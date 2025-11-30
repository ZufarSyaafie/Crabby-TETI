-- ===================================================
-- QUICK SAMPLE DATA SCRIPT
-- ===================================================
-- Script untuk menambahkan sample data dengan cepat
-- Jalankan di Supabase SQL Editor
-- ===================================================

-- IMPORTANT: Ganti user_id dengan ID user yang sudah login!
-- Cara cek: SELECT * FROM users; dan lihat kolom 'id'

-- Contoh: Jika user_id = 1, maka gunakan 1
-- Jika ada user lain, ganti dengan ID yang sesuai
DO $$
DECLARE
    v_user_id INT := 1; -- ?? GANTI DENGAN USER ID ANDA!
    v_tambak_id INT;
BEGIN
    -- Cek apakah user ada
    IF NOT EXISTS (SELECT 1 FROM users WHERE id = v_user_id) THEN
        RAISE EXCEPTION 'User dengan ID % tidak ditemukan!', v_user_id;
    END IF;

    -- Insert sample tambak jika belum ada
    INSERT INTO tambak (nama, lokasi, luas_m2, kapasitas, status, user_id) 
    VALUES 
        ('Tambak Sample A', 'Sidoarjo, Jawa Timur', 5000, 10000, 'Aktif', v_user_id)
    ON CONFLICT DO NOTHING
    RETURNING id INTO v_tambak_id;

    -- Jika tambak sudah ada, ambil ID-nya
    IF v_tambak_id IS NULL THEN
        SELECT id INTO v_tambak_id FROM tambak WHERE user_id = v_user_id LIMIT 1;
    END IF;

    RAISE NOTICE 'Using tambak_id: %', v_tambak_id;

    -- Delete old monitoring data (opsional - jika ingin fresh data)
    -- DELETE FROM monitoring_data WHERE tambak_id = v_tambak_id;

    -- Insert monitoring data untuk 5 jam terakhir
    -- Tambahkan data setiap jam
    INSERT INTO monitoring_data (tambak_id, suhu, salinitas, ph, oksigen, recorded_at) VALUES
        -- 5 jam yang lalu
        (v_tambak_id, 28.5, 30.2, 7.8, 6.5, NOW() - INTERVAL '5 hours'),
        -- 4 jam yang lalu
        (v_tambak_id, 28.8, 30.5, 7.7, 6.4, NOW() - INTERVAL '4 hours'),
        -- 3 jam yang lalu
        (v_tambak_id, 29.0, 30.8, 7.6, 6.3, NOW() - INTERVAL '3 hours'),
        -- 2 jam yang lalu
        (v_tambak_id, 29.2, 31.0, 7.5, 6.2, NOW() - INTERVAL '2 hours'),
        -- 1 jam yang lalu
        (v_tambak_id, 29.5, 31.2, 7.4, 6.1, NOW() - INTERVAL '1 hour'),
        -- Sekarang
        (v_tambak_id, 29.8, 31.5, 7.3, 6.0, NOW())
    ON CONFLICT DO NOTHING;

    RAISE NOTICE 'Sample data inserted successfully!';
    RAISE NOTICE 'Refresh your dashboard to see the chart.';
END $$;

-- Verify data
SELECT 
    md.id,
    t.nama as tambak_nama,
    md.suhu,
    md.salinitas,
    md.ph,
    md.recorded_at
FROM monitoring_data md
JOIN tambak t ON md.tambak_id = t.id
WHERE md.recorded_at >= NOW() - INTERVAL '5 hours'
ORDER BY md.recorded_at DESC;

-- ===================================================
-- TROUBLESHOOTING
-- ===================================================

-- 1. Cek apakah user ada
-- SELECT * FROM users;

-- 2. Cek apakah tambak ada untuk user
-- SELECT * FROM tambak WHERE user_id = 1; -- ganti 1 dengan user_id Anda

-- 3. Cek monitoring data
-- SELECT COUNT(*) FROM monitoring_data 
-- WHERE tambak_id IN (SELECT id FROM tambak WHERE user_id = 1)
--   AND recorded_at >= NOW() - INTERVAL '5 hours';

-- 4. Delete semua data dan start fresh (HATI-HATI!)
-- DELETE FROM monitoring_data WHERE tambak_id IN (SELECT id FROM tambak WHERE user_id = 1);
-- DELETE FROM tambak WHERE user_id = 1;

-- ===================================================
-- AFTER RUNNING THIS SCRIPT
-- ===================================================
-- 1. Refresh dashboard di aplikasi (klik tombol Refresh)
-- 2. Chart seharusnya muncul dengan 6 data points
-- 3. Check Output window di Visual Studio untuk debug logs
-- ===================================================
