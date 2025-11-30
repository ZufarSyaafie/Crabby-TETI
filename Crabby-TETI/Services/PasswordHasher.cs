using System;
using System.Security.Cryptography;
using System.Text;

namespace CrabbyTETI.Services
{
    /// Utility class untuk hashing password
    /// Menerapkan konsep ENCAPSULATION - implementation details disembunyikan
    public static class PasswordHasher
    {
        /// Hash password menggunakan SHA256
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password tidak boleh kosong", nameof(password));

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// Verify password dengan hash yang tersimpan
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
