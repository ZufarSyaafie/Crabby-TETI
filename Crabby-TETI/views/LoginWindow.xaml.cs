using System;
using System.Windows;
using System.Windows.Input;
using CrabbyTETI.ViewModels;

namespace CrabbyTETI.views
{
    /// <summary>
    /// LoginWindow Code-Behind - MVP Version
    /// Fitur: Login dengan name/email dan password
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Subscribe to ViewModel events
            _viewModel.OnLoginSuccess += HandleLoginSuccess;
            _viewModel.OnNavigateToSignUp += HandleNavigateToSignUp;
            _viewModel.OnClose += HandleClose;

            // Enable window dragging
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void HandleLoginSuccess()
        {
            // Get logged in user from ViewModel
            var loggedInUser = _viewModel.GetLoggedInUser();
            
            if (loggedInUser is not null)
            {
                // Open Dashboard window
                var dashboardWindow = new DashboardWindow(loggedInUser);
                dashboardWindow.Show();
                
                // Close login window
                this.Close();
            }
            else
            {
                MessageBox.Show("Login berhasil tetapi data user tidak ditemukan!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleNavigateToSignUp()
        {
            SignUpWindow signUpWindow = new SignUpWindow();
            signUpWindow.Show();
            this.Close();
        }

        private void HandleClose()
        {
            Application.Current.Shutdown();
        }

        // Keep event handlers for XAML compatibility during transition
        // These will delegate to ViewModel commands
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CloseCommand.Execute(null);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Update password from PasswordBox (can't bind directly due to security)
            _viewModel.Password = txtPassword.Password;
            _viewModel.LoginCommand.Execute(null);
        }

        private void LinkSignUp_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NavigateToSignUpCommand.Execute(null);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events
            _viewModel.OnLoginSuccess -= HandleLoginSuccess;
            _viewModel.OnNavigateToSignUp -= HandleNavigateToSignUp;
            _viewModel.OnClose -= HandleClose;
            
            base.OnClosed(e);
        }
    }
}