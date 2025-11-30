using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CrabbyTETI.Commands;
using CrabbyTETI.Models;
using CrabbyTETI.Services;
using CrabbyTETI.Configuration;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CrabbyTETI.ViewModels
{
    /// <summary>
    /// ViewModel untuk Dashboard
    /// Menerapkan MVVM pattern dengan data binding
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardService _dashboardService;
        private readonly User _currentUser;

        // Summary Cards Properties
        private DashboardSummary _summary = new();
        public DashboardSummary Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        // Tambak List Properties
        private ObservableCollection<Tambak> _tambakList = new();
        public ObservableCollection<Tambak> TambakList
        {
            get => _tambakList;
            set => SetProperty(ref _tambakList, value);
        }

        // Monitoring Chart Data Properties
        private ObservableCollection<MonitoringData> _chartData = new();
        public ObservableCollection<MonitoringData> ChartData
        {
            get => _chartData;
            set => SetProperty(ref _chartData, value);
        }

        // LiveCharts2 Series
        private ISeries[] _chartSeries = Array.Empty<ISeries>();
        public ISeries[] ChartSeries
        {
            get => _chartSeries;
            set => SetProperty(ref _chartSeries, value);
        }

        // LiveCharts2 Axes
        private Axis[] _xAxes = new Axis[]
        {
            new Axis
            {
                Name = "Waktu",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };
        public Axis[] XAxes
        {
            get => _xAxes;
            set => SetProperty(ref _xAxes, value);
        }

        private Axis[] _yAxes = new Axis[]
        {
            new Axis
            {
                Name = "Nilai",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12,
                MinLimit = 0
            }
        };
        public Axis[] YAxes
        {
            get => _yAxes;
            set => SetProperty(ref _yAxes, value);
        }

        // Loading State
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand AddPanenCommand { get; }

        // Events
        public event Action? OnLogout;
        public event Action? OnOpenAddPanen;

        public DashboardViewModel(User currentUser)
        {
            _currentUser = currentUser;
            _dashboardService = new DashboardService(DatabaseConfig.ConnectionString);

            // Initialize database tables
            try
            {
                _dashboardService.InitializeDatabase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize dashboard tables: {ex.Message}");
            }

            RefreshCommand = new RelayCommand(async _ => await LoadDashboardDataAsync());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            AddPanenCommand = new RelayCommand(_ => OnOpenAddPanen?.Invoke());

            // Load initial data
            _ = LoadDashboardDataAsync();
        }

        /// <summary>
        /// Load semua data dashboard
        /// </summary>
        private async Task LoadDashboardDataAsync()
        {
            IsLoading = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== LOADING DASHBOARD DATA ===");
                
                // Load summary statistics
                Summary = await _dashboardService.GetDashboardSummaryAsync(_currentUser.Id);
                System.Diagnostics.Debug.WriteLine($"Summary loaded: {Summary.TotalTambak} tambak");

                // Load tambak list
                var tambakList = await _dashboardService.GetTambakListAsync(_currentUser.Id);
                TambakList = new ObservableCollection<Tambak>(tambakList);
                System.Diagnostics.Debug.WriteLine($"Tambak list loaded: {tambakList.Count} items");

                // Load monitoring data for chart
                var monitoringData = await _dashboardService.GetMonitoringDataForChartAsync(_currentUser.Id);
                ChartData = new ObservableCollection<MonitoringData>(monitoringData);
                System.Diagnostics.Debug.WriteLine($"Monitoring data loaded: {monitoringData.Count} items");
                
                // Check if we need sample data
                if (monitoringData.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("?? No monitoring data found!");
                    System.Diagnostics.Debug.WriteLine("Possible causes:");
                    System.Diagnostics.Debug.WriteLine("1. No data in database");
                    System.Diagnostics.Debug.WriteLine("2. Data older than 5 hours");
                    System.Diagnostics.Debug.WriteLine("3. No tambak associated with user");
                    System.Diagnostics.Debug.WriteLine("\nSolution: Run the sample data SQL script (database_tambak_setup.sql)");
                    
                    // Create sample data message
                    System.Diagnostics.Debug.WriteLine("\n?? To add sample data, execute this SQL in Supabase:");
                    System.Diagnostics.Debug.WriteLine("-- See database_tambak_setup.sql for full script");
                }

                // Process data untuk chart series
                ProcessChartData(monitoringData);
                
                System.Diagnostics.Debug.WriteLine("=== DASHBOARD LOADED SUCCESSFULLY ===\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error loading dashboard data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Process monitoring data untuk chart series
        /// Menghitung rata-rata per jam dari semua tambak
        /// </summary>
        private void ProcessChartData(List<MonitoringData> data)
        {
            System.Diagnostics.Debug.WriteLine("=== PROCESSING CHART DATA ===");
            System.Diagnostics.Debug.WriteLine($"Raw data count: {data?.Count ?? 0}");
            
            if (data == null || !data.Any())
            {
                System.Diagnostics.Debug.WriteLine("? No data available for chart - ChartSeries will be empty");
                
                // Set empty chart with message
                ChartSeries = Array.Empty<ISeries>();
                
                // Create empty axes with message
                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Waktu (No Data)",
                        NamePaint = new SolidColorPaint(SKColors.Gray),
                        LabelsPaint = new SolidColorPaint(SKColors.Gray),
                        TextSize = 12,
                        Labels = new[] { "No data available" }
                    }
                };
                
                return;
            }

            // Debug: Print raw data
            System.Diagnostics.Debug.WriteLine("\nRaw Monitoring Data:");
            foreach (var item in data.Take(5)) // Print first 5 items
            {
                System.Diagnostics.Debug.WriteLine($"  - TambakId: {item.TambakId}, Suhu: {item.Suhu}, Salinitas: {item.Salinitas}, pH: {item.Ph}, Time: {item.RecordedAt}");
            }
            if (data.Count > 5)
            {
                System.Diagnostics.Debug.WriteLine($"  ... and {data.Count - 5} more items");
            }

            // Group by recorded time (rounded to hour) dan hitung rata-rata
            var groupedData = data
                .GroupBy(d => new DateTime(d.RecordedAt.Year, d.RecordedAt.Month, d.RecordedAt.Day, d.RecordedAt.Hour, 0, 0))
                .Select(g => new
                {
                    Time = g.Key,
                    TimeLabel = g.Key.ToString("HH:mm"),
                    AvgSuhu = g.Average(x => x.Suhu),
                    AvgSalinitas = g.Average(x => x.Salinitas),
                    AvgPh = g.Average(x => x.Ph)
                })
                .OrderBy(x => x.Time)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"\nGrouped data count: {groupedData.Count}");

            if (!groupedData.Any())
            {
                System.Diagnostics.Debug.WriteLine("? Grouped data is empty after processing");
                ChartSeries = Array.Empty<ISeries>();
                return;
            }

            // Debug: Print grouped data
            System.Diagnostics.Debug.WriteLine("\nGrouped Chart Data:");
            foreach (var item in groupedData)
            {
                System.Diagnostics.Debug.WriteLine($"  - Time: {item.TimeLabel}, Suhu: {item.AvgSuhu:F1}, Salinitas: {item.AvgSalinitas:F1}, pH: {item.AvgPh:F1}");
            }

            // Prepare data untuk chart
            var suhuValues = groupedData.Select(x => (double)x.AvgSuhu).ToArray();
            var salinitasValues = groupedData.Select(x => (double)x.AvgSalinitas).ToArray();
            var phValues = groupedData.Select(x => (double)x.AvgPh).ToArray();
            var timeLabels = groupedData.Select(x => x.TimeLabel).ToArray();

            System.Diagnostics.Debug.WriteLine($"\nChart Arrays:");
            System.Diagnostics.Debug.WriteLine($"  - Suhu values: [{string.Join(", ", suhuValues.Select(v => v.ToString("F1")))}]");
            System.Diagnostics.Debug.WriteLine($"  - Salinitas values: [{string.Join(", ", salinitasValues.Select(v => v.ToString("F1")))}]");
            System.Diagnostics.Debug.WriteLine($"  - pH values: [{string.Join(", ", phValues.Select(v => v.ToString("F1")))}]");
            System.Diagnostics.Debug.WriteLine($"  - Time labels: [{string.Join(", ", timeLabels)}]");

            // Update X Axis dengan labels
            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Waktu",
                    Labels = timeLabels,
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 12
                }
            };

            // Create chart series
            ChartSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Suhu (°C)",
                    Values = suhuValues,
                    Stroke = new SolidColorPaint(SKColor.Parse("#FF5722")) { StrokeThickness = 3 },
                    Fill = null,
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#FF5722")) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White)
                },
                new LineSeries<double>
                {
                    Name = "Salinitas (ppt)",
                    Values = salinitasValues,
                    Stroke = new SolidColorPaint(SKColor.Parse("#2196F3")) { StrokeThickness = 3 },
                    Fill = null,
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#2196F3")) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White)
                },
                new LineSeries<double>
                {
                    Name = "pH",
                    Values = phValues,
                    Stroke = new SolidColorPaint(SKColor.Parse("#4CAF50")) { StrokeThickness = 3 },
                    Fill = null,
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#4CAF50")) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White)
                }
            };

            System.Diagnostics.Debug.WriteLine($"\n? Chart created with {ChartSeries.Length} series");
            System.Diagnostics.Debug.WriteLine("=== END CHART PROCESSING ===\n");
        }

        /// <summary>
        /// Execute logout
        /// </summary>
        private void ExecuteLogout()
        {
            OnLogout?.Invoke();
        }

        /// <summary>
        /// Get user display info
        /// </summary>
        public string UserDisplayName => _currentUser?.Name ?? "User";
        public string UserEmail => _currentUser?.Email ?? "";
        
        /// <summary>
        /// Add data panen
        /// </summary>
        public async Task<(bool Success, string Message)> AddPanenAsync(int tambakId, DateTime tanggalPanen, decimal jumlahKg, decimal? hargaPerKg, string? keterangan)
        {
            var result = await _dashboardService.AddPanenAsync(tambakId, tanggalPanen, jumlahKg, hargaPerKg, keterangan);
            
            if (result.Success)
            {
                // Refresh data setelah insert
                await LoadDashboardDataAsync();
            }
            
            return result;
        }
    }
}
