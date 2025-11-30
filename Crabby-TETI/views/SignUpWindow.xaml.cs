using System;
using System.Windows;
using System.Windows.Input;
using CrabbyTETI.ViewModels;

namespace CrabbyTETI.views
{
    /// <summary>
    /// SignUpWindow Code-Behind - MVP Version
    /// Fitur: Register dengan name, email, password (auto-hashing)
    /// </summary>
    public partial class SignUpWindow : Window
    {
        private readonly SignUpViewModel _viewModel;

        public SignUpWindow()
        {
            InitializeComponent();
            
            _viewModel = new SignUpViewModel();
            DataContext = _viewModel;

            // Subscribe to ViewModel events
            _viewModel.OnSignUpSuccess += HandleSignUpSuccess;
            _viewModel.OnNavigateToLogin += HandleNavigateToLogin;
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

        private void HandleSignUpSuccess()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void HandleNavigateToLogin()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void HandleClose()
        {
            Application.Current.Shutdown();
        }

        // Keep event handlers for XAML compatibility during transition
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CloseCommand.Execute(null);
        }

        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            // Update passwords from PasswordBox (can't bind directly due to security)
            _viewModel.Password = txtPassword.Password;
            _viewModel.ConfirmPassword = txtConfirmPassword.Password;
            _viewModel.SignUpCommand.Execute(null);
        }

        private void LinkLogin_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NavigateToLoginCommand.Execute(null);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events
            _viewModel.OnSignUpSuccess -= HandleSignUpSuccess;
            _viewModel.OnNavigateToLogin -= HandleNavigateToLogin;
            _viewModel.OnClose -= HandleClose;
            
            base.OnClosed(e);
        }
    }
}