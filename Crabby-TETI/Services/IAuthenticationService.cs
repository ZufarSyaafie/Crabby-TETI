using System.Threading.Tasks;
using CrabbyTETI.Models;

namespace CrabbyTETI.Services
{
    /// <summary>
    /// Interface untuk Authentication Service - MVP Version
    /// Menerapkan konsep POLYMORPHISM - multiple implementation bisa menggunakan interface yang sama
    /// Disesuaikan dengan struktur Supabase: id, name, email (unik), password
    /// </summary>
    public interface IAuthenticationService
    {
        /// Method untuk inisialisasi database
        void InitializeDatabase();

        /// Method untuk login user (menggunakan email yang unik)
        Task<AuthResult> LoginAsync(string email, string password);

        /// Method untuk register user baru (name boleh duplikat, email harus unik)
        Task<AuthResult> SignUpAsync(string name, string email, string password);

        /// Method untuk cek apakah email sudah ada (email yang unik)
        Task<bool> IsEmailExistsAsync(string email);

        /// Method untuk update last login
        Task<bool> UpdateLastLoginAsync(int userId);
    }
}
