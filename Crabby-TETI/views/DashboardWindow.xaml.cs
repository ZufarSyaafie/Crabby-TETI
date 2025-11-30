using System;
using System.Linq;
using System.Windows;
using CrabbyTETI.ViewModels;
using CrabbyTETI.Models;

namespace CrabbyTETI.views
{
    /// <summary>
    /// Dashboard Window - Main window setelah login berhasil
    /// </summary>
    public partial class DashboardWindow : Window
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardWindow(User currentUser)
        {
            InitializeComponent();

            _viewModel = new DashboardViewModel(currentUser);
            DataContext = _viewModel;

            // Subscribe to events
            _viewModel.OnLogout += HandleLogout;
            _viewModel.OnOpenAddPanen += HandleOpenAddPanen;
        }

        private void HandleLogout()
        {
            // Show confirmation dialog
            var result = MessageBox.Show(
                "Apakah Anda yakin ingin logout?",
                "Konfirmasi Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Open login window
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                // Close dashboard
                this.Close();
            }
        }

        private void HandleOpenAddPanen()
        {
            // Buka dialog untuk input data panen
            var tambakList = _viewModel.TambakList.ToList();
            
            if (tambakList.Count == 0)
            {
                MessageBox.Show(
                    "Belum ada data tambak.\nSilakan tambahkan tambak terlebih dahulu melalui SQL Editor di Supabase.",
                    "Informasi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var dialog = new AddPanenDialog(_viewModel, tambakList);
            dialog.Owner = this;
            
            var result = dialog.ShowDialog();
            
            // Dialog sudah handle refresh data di ViewModel
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events
            _viewModel.OnLogout -= HandleLogout;
            _viewModel.OnOpenAddPanen -= HandleOpenAddPanen;

            base.OnClosed(e);
        }
    }
}
