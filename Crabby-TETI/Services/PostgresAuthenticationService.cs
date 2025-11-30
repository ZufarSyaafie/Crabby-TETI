using System;
using System.Threading.Tasks;
using Npgsql;
using CrabbyTETI.Models;

namespace CrabbyTETI.Services
{
    /// <summary>
    /// COPILOT GENERATED: PostgreSQL Authentication Service
    /// Menerapkan konsep INHERITANCE - inherit dari DatabaseServiceBase
    /// Menerapkan konsep POLYMORPHISM - implement IAuthenticationService interface
    /// Menerapkan konsep ENCAPSULATION - semua database operations di-encapsulate
    /// MVP Version: Disesuaikan dengan struktur Supabase (id, name, email unik, password)
    /// </summary>
    public class PostgresAuthenticationService : DatabaseServiceBase, IAuthenticationService
    {
        // ACCESS MODIFIER #1 - PRIVATE FIELD
        // Field ini PRIVATE - hanya bisa diakses dari dalam class ini
        // Digunakan untuk menyimpan informasi session atau cache
        private DateTime _lastInitializationTime;
        private bool _isDatabaseInitialized;

        /// Constructor dengan dependency injection untuk connection string
        public PostgresAuthenticationService(string connectionString) : base(connectionString)
        {
            _lastInitializationTime = DateTime.MinValue;
            _isDatabaseInitialized = false;
        }

        /// Override method untuk inisialisasi database
        /// Membuat tabel users jika belum ada dengan struktur Supabase
        /// Email adalah UNIK untuk login, name boleh sama
        public override void InitializeDatabase()
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();

                using var cmd = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS users (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(100) NOT NULL,
                        email VARCHAR(255) UNIQUE NOT NULL,
                        password VARCHAR(255) NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        last_login TIMESTAMP NULL,
                        is_active BOOLEAN DEFAULT true
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_email ON users(email);
                ", connection);

                cmd.ExecuteNonQuery();
                
                // Update private fields setelah inisialisasi berhasil
                _lastInitializationTime = DateTime.UtcNow;
                _isDatabaseInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Gagal menginisialisasi database: {ex.Message}", ex);
            }
        }

        // ACCESS MODIFIER #2 - PRIVATE METHOD
        // Method ini PRIVATE - hanya bisa dipanggil dari dalam class ini
        // Digunakan untuk validasi internal sebelum operasi database

        /// ACCESS MODIFIER - PRIVATE METHOD
        /// Method PRIVATE untuk validasi name format
        /// Hanya bisa diakses dari dalam class ini (internal validation)

        private bool ValidateNameFormat(string name)
        {
            // Name bisa berisi huruf, angka, underscore, space, dan karakter unicode
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Name minimal 2 karakter
            if (name.Length < 2)
                return false;

            return true;
        }

        // ACCESS MODIFIER #3 - PROTECTED METHOD
        // Method ini PROTECTED - bisa diakses dari class ini dan class turunannya
        // Berguna jika ada class yang inherit dari PostgresAuthenticationService

        /// ACCESS MODIFIER - PROTECTED METHOD
        /// Method PROTECTED untuk logging aktivitas database
        /// Bisa diakses dari class ini dan class turunannya (derived class)
        /// Contoh: Jika ada PostgresAuthenticationServiceV2 yang inherit, bisa akses method ini

        protected void LogDatabaseActivity(string activity, string details)
        {
            // Log untuk debugging atau audit
            var logMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {activity}: {details}";
            System.Diagnostics.Debug.WriteLine(logMessage);
        }

        /// Implementasi method Login
        /// Menerapkan ENCAPSULATION - detail query database disembunyikan dari caller
        /// Login menggunakan EMAIL (yang unik)
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return AuthResult.CreateFailure("Email dan password tidak boleh kosong");
                }

                // COPILOT GENERATED: Gunakan PROTECTED METHOD untuk logging
                LogDatabaseActivity("LOGIN_ATTEMPT", $"Email: {email}");

                using var connection = CreateConnection();
                await connection.OpenAsync();

                // Query untuk login dengan email (yang unik)
                using var cmd = new NpgsqlCommand(
                    "SELECT id, name, email, password, created_at, last_login, is_active " +
                    "FROM users WHERE email = @email AND is_active = true", connection);
                
                cmd.Parameters.AddWithValue("email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    LogDatabaseActivity("LOGIN_FAILED", $"User not found: {email}");
                    return AuthResult.CreateFailure("Email atau password salah");
                }

                var passwordHash = reader.GetString(3);
                
                // Verify password
                if (!PasswordHasher.VerifyPassword(password, passwordHash))
                {
                    LogDatabaseActivity("LOGIN_FAILED", $"Wrong password: {email}");
                    return AuthResult.CreateFailure("Email atau password salah");
                }

                // Create user object
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = passwordHash,
                    CreatedAt = reader.GetDateTime(4),
                    LastLogin = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6)
                };

                // Update last login
                await reader.CloseAsync();
                await UpdateLastLoginAsync(user.Id);

                LogDatabaseActivity("LOGIN_SUCCESS", $"User: {email}");
                return AuthResult.CreateSuccess(user, "Login berhasil!");
            }
            catch (Exception ex)
            {
                LogDatabaseActivity("LOGIN_ERROR", $"Error: {ex.Message}");
                return AuthResult.CreateFailure($"Terjadi kesalahan: {ex.Message}");
            }
        }

        /// Implementasi method SignUp
        /// Menerapkan ENCAPSULATION - detail query database disembunyikan dari caller
        /// Email harus unik, name boleh duplikat
        public async Task<AuthResult> SignUpAsync(string name, string email, string password)
        {
            try
            {
                // COPILOT GENERATED: Gunakan PRIVATE METHOD untuk validasi format
                if (!ValidateNameFormat(name))
                {
                    return AuthResult.CreateFailure("Format nama tidak valid. Minimal 2 karakter.");
                }

                // Validasi input
                if (string.IsNullOrWhiteSpace(name))
                    return AuthResult.CreateFailure("Nama tidak boleh kosong");

                if (name.Length < 2)
                    return AuthResult.CreateFailure("Nama minimal 2 karakter");

                if (string.IsNullOrWhiteSpace(email))
                    return AuthResult.CreateFailure("Email tidak boleh kosong");

                if (!email.Contains("@") || !email.Contains("."))
                    return AuthResult.CreateFailure("Format email tidak valid");

                if (string.IsNullOrWhiteSpace(password))
                    return AuthResult.CreateFailure("Password tidak boleh kosong");

                if (password.Length < 6)
                    return AuthResult.CreateFailure("Password minimal 6 karakter");

                // COPILOT GENERATED: Gunakan PROTECTED METHOD untuk logging
                LogDatabaseActivity("SIGNUP_ATTEMPT", $"Name: {name}, Email: {email}");

                // Cek email sudah ada (email yang harus unik)
                if (await IsEmailExistsAsync(email))
                {
                    LogDatabaseActivity("SIGNUP_FAILED", $"Email exists: {email}");
                    return AuthResult.CreateFailure("Email sudah terdaftar");
                }

                // Hash password
                var passwordHash = PasswordHasher.HashPassword(password);

                // Insert user baru
                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var cmd = new NpgsqlCommand(
                    "INSERT INTO users (name, email, password) " +
                    "VALUES (@name, @email, @password) " +
                    "RETURNING id, name, email, password, created_at, last_login, is_active",
                    connection);

                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("email", email);
                cmd.Parameters.AddWithValue("password", passwordHash);

                using var reader = await cmd.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    LogDatabaseActivity("SIGNUP_FAILED", "Failed to insert user");
                    return AuthResult.CreateFailure("Gagal membuat akun");
                }

                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    LastLogin = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6)
                };

                LogDatabaseActivity("SIGNUP_SUCCESS", $"New user created: {email}");
                return AuthResult.CreateSuccess(user, "Pendaftaran berhasil! Silakan login");
            }
            catch (Exception ex)
            {
                LogDatabaseActivity("SIGNUP_ERROR", $"Error: {ex.Message}");
                return AuthResult.CreateFailure($"Terjadi kesalahan: {ex.Message}");
            }
        }

        /// Cek apakah email sudah ada di database (email yang unik)
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var cmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM users WHERE email = @email", connection);
                
                cmd.Parameters.AddWithValue("email", email);

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// Update waktu last login user
        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var cmd = new NpgsqlCommand(
                    "UPDATE users SET last_login = CURRENT_TIMESTAMP WHERE id = @userId", connection);
                
                cmd.Parameters.AddWithValue("userId", userId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    LogDatabaseActivity("LAST_LOGIN_UPDATED", $"UserId: {userId}");
                }
                
                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
        }

        // ACCESS MODIFIER #4 - PUBLIC METHOD 
        // Method PUBLIC untuk mendapatkan informasi status database (bisa diakses dari luar)

        /// ACCESS MODIFIER - PUBLIC METHOD
        /// Method PUBLIC untuk get status database initialization
        /// Bisa diakses dari mana saja (ViewModel, Controller, etc)
        /// </summary>
        public string GetDatabaseStatus()
        {
            if (_isDatabaseInitialized)
            {
                var timeSinceInit = DateTime.UtcNow - _lastInitializationTime;
                return $"Database initialized {timeSinceInit.TotalSeconds:F0} seconds ago";
            }
            return "Database not initialized yet";
        }
    }
}
