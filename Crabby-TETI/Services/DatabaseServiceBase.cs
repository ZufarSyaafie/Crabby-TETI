using System;
using Npgsql;

namespace CrabbyTETI.Services
{
    /// Base class untuk database service
    /// Menerapkan konsep INHERITANCE - class ini akan di-inherit oleh service lain
    /// Menerapkan konsep ENCAPSULATION - connection string di-protected
    public abstract class DatabaseServiceBase
    {
        // COPILOT GENERATED: Protected field untuk connection string (ENCAPSULATION)
        protected readonly string _connectionString;

        /// Constructor untuk inisialisasi connection string
        protected DatabaseServiceBase(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// Method untuk membuat koneksi database
        /// Protected agar bisa diakses oleh derived classes
        protected NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        /// Method untuk test koneksi database
        /// Virtual method yang bisa di-override (POLYMORPHISM)
        public virtual bool TestConnection()
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// Abstract method untuk inisialisasi database
        /// Derived class HARUS mengimplementasikan method ini (POLYMORPHISM)
        public abstract void InitializeDatabase();
    }
}
