using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CrabbyTETI.Commands;
using CrabbyTETI.Services;
using CrabbyTETI.Configuration;

namespace CrabbyTETI.ViewModels
{
    /// <summary>
    /// ViewModel untuk SignUp - MVP Version
    /// Fitur: Register dengan name (boleh duplikat), email (unik), password (hashing otomatis)
    /// </summary>
    public class SignUpViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _acceptTerms;
        private string _statusMessage = string.Empty;
        private bool _isStatusError;
        private bool _isStatusVisible;

        // Authentication service dengan POLYMORPHISM (interface)
        private readonly IAuthenticationService _authService;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public bool AcceptTerms
        {
            get => _acceptTerms;
            set => SetProperty(ref _acceptTerms, value);
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

        public ICommand SignUpCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? OnSignUpSuccess;
        public event Action? OnNavigateToLogin;
        public event Action? OnClose;

        public SignUpViewModel()
        {
            // Inisialisasi authentication service (DEPENDENCY INJECTION pattern)
            _authService = new PostgresAuthenticationService(DatabaseConfig.ConnectionString);

            SignUpCommand = new RelayCommand(_ => ExecuteSignUp());
            NavigateToLoginCommand = new RelayCommand(_ => ExecuteNavigateToLogin());
            CloseCommand = new RelayCommand(_ => ExecuteClose());
        }

        /// Method signup yang menggunakan database PostgreSQL
        /// Menerapkan ENCAPSULATION - detail implementasi disembunyikan
        /// MVP: name (boleh duplikat), email (unik), password (password otomatis di-hash di service)
        private async void ExecuteSignUp()
        {
            // Validasi basic
            if (string.IsNullOrWhiteSpace(Name))
            {
                ShowStatus("Nama tidak boleh kosong", true);
                return;
            }

            if (Name.Length < 2)
            {
                ShowStatus("Nama minimal 2 karakter", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowStatus("Email tidak boleh kosong", true);
                return;
            }

            if (!IsValidEmail(Email))
            {
                ShowStatus("Format email tidak valid", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowStatus("Password tidak boleh kosong", true);
                return;
            }

            if (Password.Length < 6)
            {
                ShowStatus("Password minimal 6 karakter", true);
                return;
            }

            if (Password != ConfirmPassword)
            {
                ShowStatus("Password dan konfirmasi password tidak cocok", true);
                return;
            }

            if (!AcceptTerms)
            {
                ShowStatus("Anda harus menyetujui Syarat dan Ketentuan", true);
                return;
            }

            try
            {
                ShowStatus("Memproses pendaftaran...", false);

                // Panggil authentication service (menggunakan POLYMORPHISM)
                // SignUpAsync sekarang hanya menerima 3 parameter: name (boleh duplikat), email (unik), password
                var result = await _authService.SignUpAsync(Name, Email, Password);

                if (result.Success)
                {
                    ShowStatus(result.Message, false);

                    // Tunggu sebentar lalu pindah ke halaman login
                    System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            OnSignUpSuccess?.Invoke();
                        });
                    });
                }
                else
                {
                    ShowStatus(result.Message, true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Terjadi kesalahan: {ex.Message}", true);
            }
        }

        private void ExecuteNavigateToLogin()
        {
            OnNavigateToLogin?.Invoke();
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

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
