using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using CrabbyTETI.Models;
using CrabbyTETI.Configuration;

namespace CrabbyTETI.Services
{
    /// <summary>
    /// Service untuk mengambil data dashboard tambak
    /// Menerapkan ENCAPSULATION dan INHERITANCE dari DatabaseServiceBase
    /// </summary>
    public class DashboardService : DatabaseServiceBase
    {
        public DashboardService(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Get summary statistics untuk dashboard cards
        /// </summary>
        public async Task<DashboardSummary> GetDashboardSummaryAsync(int userId)
        {
            var summary = new DashboardSummary();

            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                // Query untuk semua statistik summary
                using var cmd = new NpgsqlCommand(@"
                    -- Total tambak aktif
                    SELECT COUNT(*) as total_tambak 
                    FROM tambak 
                    WHERE user_id = @userId AND status = 'Aktif';
                    
                    -- Rata-rata suhu
                    SELECT COALESCE(AVG(suhu), 0) as avg_suhu
                    FROM (
                        SELECT DISTINCT ON (tambak_id) suhu
                        FROM monitoring_data md
                        INNER JOIN tambak t ON md.tambak_id = t.id
                        WHERE t.user_id = @userId
                        ORDER BY tambak_id, recorded_at DESC
                    ) latest_data;
                    
                    -- Rata-rata salinitas
                    SELECT COALESCE(AVG(salinitas), 0) as avg_salinitas
                    FROM (
                        SELECT DISTINCT ON (tambak_id) salinitas
                        FROM monitoring_data md
                        INNER JOIN tambak t ON md.tambak_id = t.id
                        WHERE t.user_id = @userId
                        ORDER BY tambak_id, recorded_at DESC
                    ) latest_data;
                    
                    -- Rata-rata pH
                    SELECT COALESCE(AVG(ph), 0) as avg_ph
                    FROM (
                        SELECT DISTINCT ON (tambak_id) ph
                        FROM monitoring_data md
                        INNER JOIN tambak t ON md.tambak_id = t.id
                        WHERE t.user_id = @userId
                        ORDER BY tambak_id, recorded_at DESC
                    ) latest_data;
                    
                    -- Total panen bulan ini
                    SELECT COALESCE(SUM(jumlah_kg), 0) as total_panen
                    FROM panen p
                    INNER JOIN tambak t ON p.tambak_id = t.id
                    WHERE t.user_id = @userId
                      AND EXTRACT(MONTH FROM tanggal_panen) = EXTRACT(MONTH FROM CURRENT_DATE)
                      AND EXTRACT(YEAR FROM tanggal_panen) = EXTRACT(YEAR FROM CURRENT_DATE);
                ", connection);

                cmd.Parameters.AddWithValue("userId", userId);

                using var reader = await cmd.ExecuteReaderAsync();

                // Read total tambak
                if (await reader.ReadAsync())
                {
                    summary.TotalTambak = reader.GetInt32(0);
                }

                // Read rata-rata suhu
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    summary.RataSuhu = reader.GetDecimal(0);
                }

                // Read rata-rata salinitas
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    summary.RataSalinitas = reader.GetDecimal(0);
                }

                // Read rata-rata pH
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    summary.RataPh = reader.GetDecimal(0);
                }

                // Read total panen bulan ini
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    summary.TotalPanenBulanIni = reader.GetDecimal(0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting dashboard summary: {ex.Message}");
            }

            return summary;
        }

        /// <summary>
        /// Get list of tambak dengan data monitoring terkini
        /// </summary>
        public async Task<List<Tambak>> GetTambakListAsync(int userId)
        {
            var tambakList = new List<Tambak>();

            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    SELECT 
                        t.id,
                        t.nama,
                        t.lokasi,
                        t.luas_m2,
                        t.kapasitas,
                        t.status,
                        m.suhu as suhu_terkini,
                        m.salinitas as salinitas_terkini,
                        m.ph as ph_terkini,
                        m.recorded_at as last_update,
                        t.created_at,
                        t.updated_at,
                        COALESCE(p.total_panen_tahun_ini, 0) as total_panen_tahun_ini,
                        COALESCE(p.pendapatan_tahun_ini, 0) as pendapatan_tahun_ini
                    FROM tambak t
                    LEFT JOIN LATERAL (
                        SELECT suhu, salinitas, ph, recorded_at
                        FROM monitoring_data
                        WHERE tambak_id = t.id
                        ORDER BY recorded_at DESC
                        LIMIT 1
                    ) m ON true
                    LEFT JOIN LATERAL (
                        SELECT 
                            SUM(jumlah_kg) as total_panen_tahun_ini,
                            SUM(total_nilai) as pendapatan_tahun_ini
                        FROM panen
                        WHERE tambak_id = t.id
                          AND EXTRACT(YEAR FROM tanggal_panen) = EXTRACT(YEAR FROM CURRENT_DATE)
                    ) p ON true
                    WHERE t.user_id = @userId
                    ORDER BY t.nama
                ", connection);

                cmd.Parameters.AddWithValue("userId", userId);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var tambak = new Tambak
                    {
                        Id = reader.GetInt32(0),
                        Nama = reader.GetString(1),
                        Lokasi = reader.GetString(2),
                        LuasM2 = reader.GetDecimal(3),
                        Kapasitas = reader.GetInt32(4),
                        Status = reader.GetString(5),
                        SuhuTerkini = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                        SalinitasTerkini = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                        PhTerkini = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                        LastUpdate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        CreatedAt = reader.GetDateTime(10),
                        UpdatedAt = reader.GetDateTime(11),
                        TotalPanenTahunIni = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12),
                        PendapatanTahunIni = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13)
                    };

                    tambakList.Add(tambak);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting tambak list: {ex.Message}");
            }

            return tambakList;
        }

        /// <summary>
        /// Get monitoring data untuk chart (5 jam terakhir)
        /// </summary>
        public async Task<List<MonitoringData>> GetMonitoringDataForChartAsync(int userId)
        {
            var monitoringList = new List<MonitoringData>();

            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    SELECT 
                        md.id,
                        md.tambak_id,
                        md.suhu,
                        md.salinitas,
                        md.ph,
                        md.oksigen,
                        md.recorded_at
                    FROM monitoring_data md
                    INNER JOIN tambak t ON md.tambak_id = t.id
                    WHERE t.user_id = @userId
                      AND md.recorded_at >= NOW() - INTERVAL '5 hours'
                    ORDER BY md.recorded_at ASC
                ", connection);

                cmd.Parameters.AddWithValue("userId", userId);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var data = new MonitoringData
                    {
                        Id = reader.GetInt32(0),
                        TambakId = reader.GetInt32(1),
                        Suhu = reader.GetDecimal(2),
                        Salinitas = reader.GetDecimal(3),
                        Ph = reader.GetDecimal(4),
                        Oksigen = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                        RecordedAt = reader.GetDateTime(6)
                    };

                    monitoringList.Add(data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting monitoring data: {ex.Message}");
            }

            return monitoringList;
        }

        /// <summary>
        /// Initialize database tables untuk dashboard
        /// </summary>
        public override void InitializeDatabase()
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();

                // Create tables for dashboard (sama seperti di SQL file)
                using var cmd = new NpgsqlCommand(@"
                    -- Tabel tambak
                    CREATE TABLE IF NOT EXISTS tambak (
                        id SERIAL PRIMARY KEY,
                        nama VARCHAR(100) NOT NULL,
                        lokasi VARCHAR(255) NOT NULL,
                        luas_m2 DECIMAL(10,2) NOT NULL,
                        kapasitas INT NOT NULL,
                        status VARCHAR(20) DEFAULT 'Aktif',
                        user_id INT NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS idx_tambak_user ON tambak(user_id);
                    CREATE INDEX IF NOT EXISTS idx_tambak_status ON tambak(status);

                    -- Tabel monitoring_data
                    CREATE TABLE IF NOT EXISTS monitoring_data (
                        id SERIAL PRIMARY KEY,
                        tambak_id INT NOT NULL,
                        suhu DECIMAL(5,2) NOT NULL,
                        salinitas DECIMAL(5,2) NOT NULL,
                        ph DECIMAL(4,2) NOT NULL,
                        oksigen DECIMAL(5,2),
                        recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (tambak_id) REFERENCES tambak(id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS idx_monitoring_tambak ON monitoring_data(tambak_id);
                    CREATE INDEX IF NOT EXISTS idx_monitoring_recorded_at ON monitoring_data(recorded_at DESC);

                    -- Tabel panen
                    CREATE TABLE IF NOT EXISTS panen (
                        id SERIAL PRIMARY KEY,
                        tambak_id INT NOT NULL,
                        tanggal_panen DATE NOT NULL,
                        jumlah_kg DECIMAL(10,2) NOT NULL,
                        harga_per_kg DECIMAL(12,2),
                        total_nilai DECIMAL(15,2),
                        keterangan TEXT,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (tambak_id) REFERENCES tambak(id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS idx_panen_tambak ON panen(tambak_id);
                    CREATE INDEX IF NOT EXISTS idx_panen_tanggal ON panen(tanggal_panen DESC);
                ", connection);

                cmd.ExecuteNonQuery();

                System.Diagnostics.Debug.WriteLine("Dashboard tables initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing dashboard tables: {ex.Message}");
            }
        }
        
        /// <summary>
        /// INSERT DATA PANEN
        /// </summary>
        public async Task<(bool Success, string Message)> AddPanenAsync(int tambakId, DateTime tanggalPanen, decimal jumlahKg, decimal? hargaPerKg, string? keterangan)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                decimal? totalNilai = hargaPerKg.HasValue ? jumlahKg * hargaPerKg.Value : null;

                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO panen (tambak_id, tanggal_panen, jumlah_kg, harga_per_kg, total_nilai, keterangan)
                    VALUES (@tambakId, @tanggalPanen, @jumlahKg, @hargaPerKg, @totalNilai, @keterangan)
                ", connection);

                cmd.Parameters.AddWithValue("tambakId", tambakId);
                cmd.Parameters.AddWithValue("tanggalPanen", tanggalPanen);
                cmd.Parameters.AddWithValue("jumlahKg", jumlahKg);
                cmd.Parameters.AddWithValue("hargaPerKg", (object?)hargaPerKg ?? DBNull.Value);
                cmd.Parameters.AddWithValue("totalNilai", (object?)totalNilai ?? DBNull.Value);
                cmd.Parameters.AddWithValue("keterangan", (object?)keterangan ?? DBNull.Value);

                var result = await cmd.ExecuteNonQueryAsync();
                
                return result > 0 
                    ? (true, "Data panen berhasil ditambahkan!") 
                    : (false, "Gagal menambahkan data panen");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
