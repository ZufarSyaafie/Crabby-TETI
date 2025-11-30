-- ===================================================
-- SUPABASE DATABASE SETUP - Crabby-TETI Dashboard
-- ===================================================
-- Tabel untuk data tambak dan monitoring
-- ===================================================

-- ===================================================
-- 1. TABEL TAMBAK
-- ===================================================
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

-- ===================================================
-- 2. TABEL MONITORING DATA
-- ===================================================
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

-- ===================================================
-- 3. TABEL PANEN
-- ===================================================
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

-- ===================================================
-- SAMPLE DATA untuk TESTING
-- ===================================================

-- Insert sample tambak (ganti user_id sesuai dengan user yang sudah register)
INSERT INTO tambak (nama, lokasi, luas_m2, kapasitas, status, user_id) VALUES 
('Tambak A', 'Sidoarjo, Jawa Timur', 5000, 10000, 'Aktif', 1),
('Tambak B', 'Gresik, Jawa Timur', 3500, 7000, 'Aktif', 1),
('Tambak C', 'Lamongan, Jawa Timur', 4200, 8500, 'Maintenance', 1),
('Tambak D', 'Tuban, Jawa Timur', 6000, 12000, 'Aktif', 1),
('Tambak E', 'Pasuruan, Jawa Timur', 2800, 5500, 'Tidak Aktif', 1)
ON CONFLICT DO NOTHING;

-- Insert sample monitoring data (5 jam terakhir untuk setiap tambak)
-- Data untuk Tambak A
INSERT INTO monitoring_data (tambak_id, suhu, salinitas, ph, oksigen, recorded_at) VALUES
(1, 28.5, 30.2, 7.8, 6.5, NOW() - INTERVAL '5 hours'),
(1, 28.8, 30.5, 7.7, 6.4, NOW() - INTERVAL '4 hours'),
(1, 29.0, 30.8, 7.6, 6.3, NOW() - INTERVAL '3 hours'),
(1, 29.2, 31.0, 7.5, 6.2, NOW() - INTERVAL '2 hours'),
(1, 29.5, 31.2, 7.4, 6.1, NOW() - INTERVAL '1 hour'),
(1, 29.8, 31.5, 7.3, 6.0, NOW())
ON CONFLICT DO NOTHING;

-- Data untuk Tambak B
INSERT INTO monitoring_data (tambak_id, suhu, salinitas, ph, oksigen, recorded_at) VALUES
(2, 27.5, 29.5, 7.9, 6.8, NOW() - INTERVAL '5 hours'),
(2, 27.8, 29.8, 7.8, 6.7, NOW() - INTERVAL '4 hours'),
(2, 28.0, 30.0, 7.7, 6.6, NOW() - INTERVAL '3 hours'),
(2, 28.2, 30.2, 7.6, 6.5, NOW() - INTERVAL '2 hours'),
(2, 28.5, 30.5, 7.5, 6.4, NOW() - INTERVAL '1 hour'),
(2, 28.8, 30.8, 7.4, 6.3, NOW())
ON CONFLICT DO NOTHING;

-- Data untuk Tambak C
INSERT INTO monitoring_data (tambak_id, suhu, salinitas, ph, oksigen, recorded_at) VALUES
(3, 26.5, 28.5, 8.0, 7.0, NOW() - INTERVAL '5 hours'),
(3, 26.8, 28.8, 7.9, 6.9, NOW() - INTERVAL '4 hours'),
(3, 27.0, 29.0, 7.8, 6.8, NOW() - INTERVAL '3 hours'),
(3, 27.2, 29.2, 7.7, 6.7, NOW() - INTERVAL '2 hours'),
(3, 27.5, 29.5, 7.6, 6.6, NOW() - INTERVAL '1 hour'),
(3, 27.8, 29.8, 7.5, 6.5, NOW())
ON CONFLICT DO NOTHING;

-- Data untuk Tambak D
INSERT INTO monitoring_data (tambak_id, suhu, salinitas, ph, oksigen, recorded_at) VALUES
(4, 30.0, 32.0, 7.2, 5.8, NOW() - INTERVAL '5 hours'),
(4, 30.2, 32.2, 7.3, 5.9, NOW() - INTERVAL '4 hours'),
(4, 30.5, 32.5, 7.4, 6.0, NOW() - INTERVAL '3 hours'),
(4, 30.8, 32.8, 7.5, 6.1, NOW() - INTERVAL '2 hours'),
(4, 31.0, 33.0, 7.6, 6.2, NOW() - INTERVAL '1 hour'),
(4, 31.2, 33.2, 7.7, 6.3, NOW())
ON CONFLICT DO NOTHING;

-- Insert sample panen data (bulan ini)
INSERT INTO panen (tambak_id, tanggal_panen, jumlah_kg, harga_per_kg, total_nilai, keterangan) VALUES
(1, CURRENT_DATE - INTERVAL '5 days', 350.5, 85000, 29792500, 'Panen rutin'),
(2, CURRENT_DATE - INTERVAL '3 days', 280.0, 85000, 23800000, 'Panen rutin'),
(1, CURRENT_DATE - INTERVAL '10 days', 420.8, 82000, 34505600, 'Panen besar'),
(4, CURRENT_DATE - INTERVAL '15 days', 520.0, 88000, 45760000, 'Panen berkualitas tinggi')
ON CONFLICT DO NOTHING;

-- ===================================================
-- QUERY UNTUK DASHBOARD
-- ===================================================

-- 1. Summary Statistics
-- Total Tambak
SELECT COUNT(*) as total_tambak FROM tambak WHERE status = 'Aktif';

-- Rata-rata Suhu (dari data terbaru setiap tambak)
SELECT AVG(suhu) as avg_suhu
FROM (
    SELECT DISTINCT ON (tambak_id) suhu
    FROM monitoring_data
    ORDER BY tambak_id, recorded_at DESC
) latest_data;

-- Rata-rata Salinitas
SELECT AVG(salinitas) as avg_salinitas
FROM (
    SELECT DISTINCT ON (tambak_id) salinitas
    FROM monitoring_data
    ORDER BY tambak_id, recorded_at DESC
) latest_data;

-- Rata-rata pH
SELECT AVG(ph) as avg_ph
FROM (
    SELECT DISTINCT ON (tambak_id) ph
    FROM monitoring_data
    ORDER BY tambak_id, recorded_at DESC
) latest_data;

-- Total Panen Bulan Ini (kg)
SELECT COALESCE(SUM(jumlah_kg), 0) as total_panen_bulan_ini
FROM panen
WHERE EXTRACT(MONTH FROM tanggal_panen) = EXTRACT(MONTH FROM CURRENT_DATE)
  AND EXTRACT(YEAR FROM tanggal_panen) = EXTRACT(YEAR FROM CURRENT_DATE);

-- 2. Daftar Tambak dengan Status
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
    m.recorded_at as last_update
FROM tambak t
LEFT JOIN LATERAL (
    SELECT suhu, salinitas, ph, recorded_at
    FROM monitoring_data
    WHERE tambak_id = t.id
    ORDER BY recorded_at DESC
    LIMIT 1
) m ON true
ORDER BY t.nama;

-- 3. Monitoring Data 5 Jam Terakhir (untuk chart)
SELECT 
    tambak_id,
    suhu,
    salinitas,
    ph,
    recorded_at
FROM monitoring_data
WHERE recorded_at >= NOW() - INTERVAL '5 hours'
ORDER BY recorded_at ASC;

-- ===================================================
-- NOTES
-- ===================================================
-- 1. Data monitoring di-record setiap jam (atau sesuai kebutuhan)
-- 2. Status tambak: 'Aktif', 'Maintenance', 'Tidak Aktif'
-- 3. Grafik menampilkan data 5 jam terakhir
-- 4. Panen di-track per tambak dengan tanggal
-- ===================================================
