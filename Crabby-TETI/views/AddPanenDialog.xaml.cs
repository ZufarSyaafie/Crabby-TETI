using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using CrabbyTETI.Models;
using CrabbyTETI.ViewModels;

namespace CrabbyTETI.views
{
    public partial class AddPanenDialog : Window
    {
        private readonly DashboardViewModel _dashboardViewModel;

        public AddPanenDialog(DashboardViewModel dashboardViewModel, List<Tambak> tambakList)
        {
            InitializeComponent();
            _dashboardViewModel = dashboardViewModel;

            // Populate combo box dengan tambak list
            cmbTambak.ItemsSource = tambakList;
            if (tambakList.Any())
            {
                cmbTambak.SelectedIndex = 0;
            }

            // Set default date to today
            dpTanggalPanen.SelectedDate = DateTime.Today;

            // Add event handlers untuk auto-calculate total nilai
            txtJumlahKg.TextChanged += CalculateTotalNilai;
            txtHargaPerKg.TextChanged += CalculateTotalNilai;
        }

        private void CalculateTotalNilai(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtJumlahKg.Text, out decimal jumlahKg) &&
                decimal.TryParse(txtHargaPerKg.Text, out decimal hargaPerKg))
            {
                decimal totalNilai = jumlahKg * hargaPerKg;
                txtTotalNilai.Text = $"Rp {totalNilai:N0}";
            }
            else
            {
                txtTotalNilai.Text = "Rp 0";
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validasi input
            if (cmbTambak.SelectedValue == null)
            {
                ShowError("Pilih tambak terlebih dahulu");
                return;
            }

            if (dpTanggalPanen.SelectedDate == null)
            {
                ShowError("Pilih tanggal panen");
                return;
            }

            if (!decimal.TryParse(txtJumlahKg.Text, out decimal jumlahKg) || jumlahKg <= 0)
            {
                ShowError("Jumlah panen harus berupa angka positif");
                return;
            }

            // Harga per kg opsional
            decimal? hargaPerKg = null;
            if (!string.IsNullOrWhiteSpace(txtHargaPerKg.Text))
            {
                if (decimal.TryParse(txtHargaPerKg.Text, out decimal harga) && harga > 0)
                {
                    hargaPerKg = harga;
                }
                else
                {
                    ShowError("Harga per kg harus berupa angka positif atau kosongkan");
                    return;
                }
            }

            int tambakId = (int)cmbTambak.SelectedValue;
            DateTime tanggalPanen = dpTanggalPanen.SelectedDate.Value;
            string? keterangan = string.IsNullOrWhiteSpace(txtKeterangan.Text) ? null : txtKeterangan.Text;

            try
            {
                var result = await _dashboardViewModel.AddPanenAsync(
                    tambakId,
                    tanggalPanen,
                    jumlahKg,
                    hargaPerKg,
                    keterangan
                );

                if (result.Success)
                {
                    var tambakNama = ((Tambak)cmbTambak.SelectedItem).Nama;
                    var message = $"Data panen berhasil ditambahkan!\n\n" +
                                  $"Tambak: {tambakNama}\n" +
                                  $"Jumlah: {jumlahKg:N1} kg\n" +
                                  $"Tanggal: {tanggalPanen:dd/MM/yyyy}";

                    if (hargaPerKg.HasValue)
                    {
                        decimal totalNilai = jumlahKg * hargaPerKg.Value;
                        message += $"\nTotal Nilai: Rp {totalNilai:N0}";
                    }

                    MessageBox.Show(message, "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            txtStatusMessage.Text = message;
            txtStatusMessage.Visibility = Visibility.Visible;
        }
    }
}
