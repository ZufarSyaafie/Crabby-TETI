-- ===================================================
-- SUPABASE DATABASE SETUP - Crabby-TETI MVP
-- ===================================================
-- Tabel users untuk authentication
-- EMAIL adalah UNIK untuk login (bukan name)
-- Name boleh duplikat
-- ===================================================

-- Drop table jika sudah ada (hati-hati, ini akan hapus semua data!)
-- DROP TABLE IF EXISTS users;

-- Create users table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP NULL,
    is_active BOOLEAN DEFAULT true
);

-- Create index untuk performa query login
CREATE INDEX IF NOT EXISTS idx_email ON users(email);

-- ===================================================
-- STRUKTUR TABEL LENGKAP
-- ===================================================
-- id          : Auto-increment primary key
-- name        : Nama user (BOLEH DUPLIKAT, tidak unik)
-- email       : Email user (HARUS UNIK, untuk login)
-- password    : Password hash (BCrypt, 60 karakter)
-- created_at  : Waktu registrasi
-- last_login  : Waktu login terakhir
-- is_active   : Status aktif user
-- ===================================================

-- ===================================================
-- CONTOH DATA (OPTIONAL - untuk testing)
-- ===================================================
-- Password asli: "password123"
-- Hash BCrypt dengan work factor 12

-- INSERT INTO users (name, email, password) VALUES 
-- (
--     'John Doe',
--     'john@example.com',
--     '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYk7H7GxB3u'
-- );

-- INSERT INTO users (name, email, password) VALUES 
-- (
--     'Jane Smith',
--     'jane@example.com',
--     '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYk7H7GxB3u'
-- );

-- Perhatikan: Kedua user bisa punya nama yang sama jika diinginkan
-- Yang penting email harus berbeda (unik)

-- ===================================================
-- QUERY UNTUK CEK DATA
-- ===================================================

-- Lihat semua user
-- SELECT * FROM users ORDER BY created_at DESC;

-- Lihat user berdasarkan email
-- SELECT * FROM users WHERE email = 'john@example.com';

-- Count total user
-- SELECT COUNT(*) as total_users FROM users;

-- Lihat user yang login hari ini
-- SELECT * FROM users WHERE last_login::date = CURRENT_DATE;

-- ===================================================
-- QUERY UNTUK MAINTENANCE
-- ===================================================

-- Update last_login setelah user login berhasil
-- UPDATE users SET last_login = CURRENT_TIMESTAMP WHERE id = 1;

-- Soft delete user (set is_active = false)
-- UPDATE users SET is_active = false WHERE id = 1;

-- Hard delete user (HATI-HATI!)
-- DELETE FROM users WHERE id = 1;

-- Reset semua data (HATI-HATI!)
-- TRUNCATE TABLE users RESTART IDENTITY;

-- ===================================================
-- NOTES PENTING
-- ===================================================
-- 1. EMAIL adalah UNIQUE constraint - tidak boleh duplikat
-- 2. NAME boleh duplikat - banyak orang bisa bernama sama
-- 3. PASSWORD selalu dalam bentuk hash (BCrypt) - JANGAN simpan plain text
-- 4. Index pada email untuk performa login yang cepat
-- 5. Aplikasi WPF akan otomatis create table ini saat pertama kali jalan
-- ===================================================
