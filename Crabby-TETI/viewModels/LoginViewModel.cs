using System;
using System.Windows;
using System.Windows.Input;
using CrabbyTETI.Commands;
using CrabbyTETI.Services;
using CrabbyTETI.Configuration;
using CrabbyTETI.Models;

namespace CrabbyTETI.ViewModels
{
    /// <summary>
    /// ViewModel untuk Login - MVP Version
    /// Fitur: Login dengan email (unik), password hashing
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isStatusError;
        private bool _isStatusVisible;
        private bool _rememberMe;

        // Authentication service dengan POLYMORPHISM (interface)
        private readonly IAuthenticationService _authService;
        
        // Store logged in user
        private User? _loggedInUser;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsStatusError
        {
            get => _isStatusError;
            set => SetProperty(ref _isStatusError, value);
        }

        public bool IsStatusVisible
        {
            get => _isStatusVisible;
            set => SetProperty(ref _isStatusVisible, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToSignUpCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? OnLoginSuccess;
        public event Action? OnNavigateToSignUp;
        public event Action? OnClose;

        public LoginViewModel()
        {
            // Inisialisasi authentication service (DEPENDENCY INJECTION pattern)
            _authService = new PostgresAuthenticationService(DatabaseConfig.ConnectionString);
            
            // Inisialisasi database (create table if not exists)
            try
            {
                _authService.InitializeDatabase();
            }
            catch (Exception ex)
            {
                ShowStatus($"Gagal koneksi database: {ex.Message}", true);
            }

            LoginCommand = new RelayCommand(_ => ExecuteLogin(), _ => CanExecuteLogin());
            NavigateToSignUpCommand = new RelayCommand(_ => ExecuteNavigateToSignUp());
            CloseCommand = new RelayCommand(_ => ExecuteClose());
        }

        private bool CanExecuteLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        /// Method login yang menggunakan database PostgreSQL
        /// Menerapkan ENCAPSULATION - detail implementasi disembunyikan
        /// Login dengan EMAIL yang unik
        private async void ExecuteLogin()
        {
            // Validasi input
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowStatus("Email tidak boleh kosong", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowStatus("Password tidak boleh kosong", true);
                return;
            }

            try
            {
                ShowStatus("Memproses login...", false);

                // Panggil authentication service (menggunakan POLYMORPHISM)
                var result = await _authService.LoginAsync(Email, Password);

                if (result.Success)
                {
                    ShowStatus(result.Message, false);
                    
                    // Store logged in user
                    _loggedInUser = result.User;
                    
                    // TODO: Simpan session jika RememberMe checked
                    if (RememberMe && result.User != null)
                    {
                        // Bisa implement session management di sini
                    }

                    // Delay sedikit untuk menampilkan pesan sukses
                    await System.Threading.Tasks.Task.Delay(500);
                    
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    ShowStatus(result.Message, true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", true);
            }
        }

        private void ExecuteNavigateToSignUp()
        {
            OnNavigateToSignUp?.Invoke();
        }

        private void ExecuteClose()
        {
            OnClose?.Invoke();
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusMessage = message;
            IsStatusError = isError;
            IsStatusVisible = true;
        }
        
        /// <summary>
        /// Get logged in user after successful login
        /// </summary>
        public User? GetLoggedInUser()
        {
            return _loggedInUser;
        }
    }
}
