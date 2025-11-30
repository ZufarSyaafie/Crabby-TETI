using System;

namespace CrabbyTETI.Configuration
{
    /// <summary>
    /// Database configuration with multiple connection options
    /// Pilih connection string yang sesuai dengan environment Anda
    /// </summary>
    public static class DatabaseConfig
    {
        // ====================================================================
        // OPTION 1: Connection Pooler (RECOMMENDED for Production)
        // ====================================================================
        // Port: 6543 (Transaction Mode - untuk apps dengan banyak koneksi)
        // Format: aws-0-{region}.pooler.supabase.com
        private static string ConnectionStringPooler =
            "Host=aws-1-ap-southeast-1.pooler.supabase.com;" +
            "Port=6543;" +
            "Database=postgres;" +
            "Username=postgres.tffeswefrjkrnkrkdgyl;" +  // Format: postgres.{project-ref}
            "Password=padmanaba212;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;" +
            "Timeout=30;" +
            "Command Timeout=30;";

        // ====================================================================
        // OPTION 2: Direct Connection (untuk development/testing)
        // ====================================================================
        // Port: 5432 (Direct connection ke database)
        // Gunakan ini jika pooler tidak work
        private static string ConnectionStringDirect =
            "Host=db.tffeswefrjkrnkrkdgyl.supabase.co;" +
            "Port=5432;" +
            "Database=postgres;" +
            "Username=postgres;" +
            "Password=padmanaba212;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;" +
            "Timeout=30;" +
            "Command Timeout=30;";

        // ====================================================================
        // OPTION 3: Session Pooler
        // ====================================================================
        // Port: 5432 (Session Mode - untuk long-running connections)
        private static string ConnectionStringSession =
            "Host=aws-1-ap-southeast-1.pooler.supabase.com;" +
            "Port=5432;" +
            "Database=postgres;" +
            "Username=postgres.tffeswefrjkrnkrkdgyl;" +
            "Password=padmanaba212;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;" +
            "Timeout=30;" +
            "Command Timeout=30;";

        // ====================================================================
        // OPTION 4: Connection String dari Supabase Dashboard
        // ====================================================================
        // Cara mendapatkan:
        // 1. Login ke https://supabase.com/dashboard
        // 2. Pilih project Anda
        // 3. Settings > Database
        // 4. Scroll ke "Connection String"
        // 5. Copy URI dan paste di bawah
        private static string ConnectionStringFromDashboard =
            "postgresql://postgres.tffeswefrjkrnkrkdgyl:padmanaba212@aws-1-ap-southeast-1.pooler.supabase.com:6543/postgres";

        // ====================================================================
        // ACTIVE CONNECTION STRING - Ganti ini untuk switch mode
        // ====================================================================
        
        /// <summary>
        /// Connection string yang aktif digunakan
        /// DEFAULT: Direct Connection (paling reliable untuk development)
        /// </summary>
        //public static string ConnectionString { get; set; } = ConnectionStringDirect;

        // Alternative: Uncomment untuk menggunakan Pooler
         //public static string ConnectionString { get; set; } = ConnectionStringPooler;

        // Alternative: Uncomment untuk menggunakan Session
         public static string ConnectionString { get; set; } = ConnectionStringSession;

        // ====================================================================
        // CONNECTION STRING BUILDER (untuk konfigurasi dinamis)
        // ====================================================================

        /// <summary>
        /// Build connection string secara manual
        /// Gunakan ini jika Anda punya custom requirements
        /// </summary>
        public static string BuildConnectionString(
            string host = "db.tffeswefrjkrnkrkdgyl.supabase.co",
            int port = 5432,
            string database = "postgres",
            string username = "postgres",
            string password = "padmanaba212",
            string sslMode = "Require",
            int timeout = 30)
        {
            return $"Host={host};" +
                   $"Port={port};" +
                   $"Database={database};" +
                   $"Username={username};" +
                   $"Password={password};" +
                   $"SSL Mode={sslMode};" +
                   "Trust Server Certificate=true;" +
                   $"Timeout={timeout};" +
                   $"Command Timeout={timeout};";
        }

        // ====================================================================
        // TROUBLESHOOTING HELPER
        // ====================================================================
        
        /// <summary>
        /// Get informasi connection string yang sedang aktif
        /// Berguna untuk debugging
        /// </summary>
        public static string GetConnectionInfo()
        {
            try
            {
                var parts = ConnectionString.Split(';');
                string host = "Unknown";
                string port = "Unknown";
                string database = "Unknown";
                
                foreach (var part in parts)
                {
                    if (part.StartsWith("Host=")) host = part.Replace("Host=", "");
                    if (part.StartsWith("Port=")) port = part.Replace("Port=", "");
                    if (part.StartsWith("Database=")) database = part.Replace("Database=", "");
                }
                
                return $"Connecting to: {host}:{port}/{database}";
            }
            catch
            {
                return "Unable to parse connection string";
            }
        }

        // ====================================================================
        // FALLBACK CONNECTION STRINGS (untuk automatic retry)
        // ====================================================================
        
        /// <summary>
        /// Array of fallback connection strings
        /// Sistem akan coba connect satu per satu sampai ada yang berhasil
        /// </summary>
        public static string[] FallbackConnectionStrings = new[]
        {
            ConnectionStringDirect,      // Try direct first
            ConnectionStringPooler,      // Then pooler
            ConnectionStringSession      // Then session
        };
    }
}
