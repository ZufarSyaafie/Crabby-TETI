# Crabby-TETI

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-Windows%20Presentation%20Foundation-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Supabase](https://img.shields.io/badge/Supabase-3ECF8E?style=for-the-badge&logo=supabase&logoColor=white)

</div>

## Deskripsi

Crabby-TETI adalah aplikasi desktop Windows untuk sistem monitoring dan manajemen tambak udang. Aplikasi ini menyediakan dashboard real-time untuk memantau parameter lingkungan tambak seperti suhu, salinitas, dan pH, serta manajemen data panen.

## Teknologi yang Digunakan

### Framework & Runtime

- **.NET 8.0** - Framework aplikasi modern
- **WPF (Windows Presentation Foundation)** - UI framework untuk desktop Windows
- **C# 12** - Programming language

### Database & Backend

- **PostgreSQL** - Database relasional untuk penyimpanan data
- **Supabase** - Backend-as-a-Service untuk hosting database PostgreSQL
- **Npgsql 9.0.4** - PostgreSQL data provider untuk .NET

### Libraries & Tools

- **LiveChartsCore.SkiaSharpView.WPF 2.0.0-rc2** - Library untuk visualisasi data dan chart
- **BCrypt.Net** - Library untuk hashing password (melalui PasswordHasher service)

### Design Patterns

- **MVVM (Model-View-ViewModel)** - Arsitektur aplikasi
- **Command Pattern** - Untuk handling user actions
- **Service Layer Pattern** - Pemisahan business logic
- **Repository Pattern** - Abstraksi data access

## Fitur Aplikasi

### 1. Authentication System

- **Registrasi User**
  - Validasi email unik
  - Password hashing menggunakan BCrypt
  - Validasi input (email format, password strength)
- **Login User**
  - Autentikasi dengan email dan password
  - Session management
  - Password verification dengan BCrypt

### 2. Dashboard Monitoring

- **Summary Cards**

  - Total tambak aktif
  - Rata-rata suhu real-time
  - Rata-rata salinitas
  - Rata-rata pH
  - Total panen bulan berjalan (kg)

- **Grafik Monitoring Real-Time**

  - Line chart untuk suhu, salinitas, dan pH
  - Data 5 jam terakhir
  - Update otomatis
  - Multi-series visualization dengan LiveCharts2

- **Daftar Tambak**
  - Nama dan lokasi tambak
  - Luas area (m�)
  - Kapasitas (ekor udang)
  - Status (Aktif/Maintenance/Tidak Aktif)
  - Parameter terkini per tambak

### 3. Manajemen Panen

- **Input Data Panen**
  - Tanggal panen
  - Jumlah hasil panen (kg)
  - Harga per kilogram (opsional)
  - Total nilai otomatis
  - Keterangan tambahan
- **History Panen**
  - Tracking semua data panen per tambak
  - Perhitungan total nilai panen

### 4. User Interface

- **Modern Design**

  - Material Design inspired
  - Responsive layout
  - Color-coded status indicators
  - Professional dashboard view

- **User Experience**
  - Smooth navigation
  - Loading indicators
  - Error handling dengan user-friendly messages
  - Data validation

## Struktur Database

### Tabel: users

```sql
- id (SERIAL PRIMARY KEY)
- name (VARCHAR)
- email (VARCHAR UNIQUE) - untuk login
- password (VARCHAR) - BCrypt hashed
- created_at (TIMESTAMP)
- last_login (TIMESTAMP)
- is_active (BOOLEAN)
```

### Tabel: tambak

```sql
- id (SERIAL PRIMARY KEY)
- nama (VARCHAR)
- lokasi (VARCHAR)
- luas_m2 (DECIMAL)
- kapasitas (INT)
- status (VARCHAR)
- user_id (INT FK)
- created_at (TIMESTAMP)
- updated_at (TIMESTAMP)
```

### Tabel: monitoring_data

```sql
- id (SERIAL PRIMARY KEY)
- tambak_id (INT FK)
- suhu (DECIMAL)
- salinitas (DECIMAL)
- ph (DECIMAL)
- oksigen (DECIMAL)
- recorded_at (TIMESTAMP)
```

### Tabel: panen

```sql
- id (SERIAL PRIMARY KEY)
- tambak_id (INT FK)
- tanggal_panen (DATE)
- jumlah_kg (DECIMAL)
- harga_per_kg (DECIMAL)
- total_nilai (DECIMAL)
- keterangan (TEXT)
- created_at (TIMESTAMP)
```

## Setup dan Instalasi

### Prasyarat

- Windows 10/11
- .NET 8.0 SDK atau runtime
- Visual Studio 2022 (untuk development)
- Akun Supabase (untuk database)

### Langkah Instalasi

#### 1. Clone Repository

```bash
git clone <repository-url>
cd Crabby-TETI
```

#### 2. Konfigurasi Database

##### 2.1. Setup Supabase

1. Buat akun di [https://supabase.com](https://supabase.com)
2. Buat project baru
3. Dapatkan connection string dari Dashboard > Settings > Database

##### 2.2. Konfigurasi Connection String

Edit file `Configuration/DatabaseConfig.cs`:

```csharp
// Pilih salah satu mode connection:

// OPTION 1: Direct Connection (Recommended untuk Development)
public static string ConnectionString { get; set; } = ConnectionStringDirect;

// OPTION 2: Connection Pooler (Recommended untuk Production)
public static string ConnectionString { get; set; } = ConnectionStringPooler;

// OPTION 3: Session Pooler (untuk Long-Running Connections)
public static string ConnectionString { get; set; } = ConnectionStringSession;
```

Update nilai connection string dengan kredensial Supabase Anda:

```csharp
private static string ConnectionStringDirect =
    "Host=db.YOUR-PROJECT-REF.supabase.co;" +
    "Port=5432;" +
    "Database=postgres;" +
    "Username=postgres;" +
    "Password=YOUR-PASSWORD;" +
    "SSL Mode=Require;" +
    "Trust Server Certificate=true;" +
    "Timeout=30;" +
    "Command Timeout=30;";
```

##### 2.3. Setup Database Schema

Jalankan SQL scripts berikut di Supabase SQL Editor:

1. **Setup tabel users:**

```bash
# Jalankan file: database_setup.sql
```

2. **Setup tabel tambak & monitoring:**

```bash
# Jalankan file: database_tambak_setup.sql
```

3. **Insert sample data (opsional):**

```bash
# Jalankan file: quick_sample_data.sql
```

#### 3. Build Aplikasi

##### Menggunakan Visual Studio

1. Buka `Crabby-TETI.sln`
2. Restore NuGet packages (otomatis)
3. Build solution (Ctrl+Shift+B)
4. Run aplikasi (F5)

##### Menggunakan .NET CLI

```bash
# Restore dependencies
dotnet restore

# Build aplikasi
dotnet build

# Run aplikasi
dotnet run --project Crabby-TETI/Crabby-TETI.csproj
```

## Penggunaan Aplikasi

### Pertama Kali Menggunakan

1. **Registrasi Akun**

   - Jalankan aplikasi
   - Klik "Sign Up"
   - Isi nama, email, dan password
   - Klik "Register"

2. **Login**

   - Masukkan email dan password
   - Klik "Login"

3. **Dashboard**
   - Lihat summary cards untuk overview
   - Pantau grafik monitoring real-time
   - Browse daftar tambak
   - Tambah data panen melalui tombol "Tambah Panen"

### Tips Penggunaan

- **Mode Connection:** Gunakan Direct Connection untuk development, Pooler untuk production
- **Sample Data:** Jalankan `quick_sample_data.sql` untuk mendapatkan data contoh
- **Refresh Data:** Klik tombol refresh untuk update data terbaru
- **Monitoring:** Data monitoring menampilkan 5 jam terakhir

## Troubleshooting

### Connection Issues

**Error: "Connection timeout"**

- Cek koneksi internet
- Verifikasi connection string di `DatabaseConfig.cs`
- Coba ganti mode connection (Direct/Pooler/Session)

**Error: "Authentication failed"**

- Pastikan username dan password benar
- Cek Supabase dashboard untuk kredensial yang valid

### Data Issues

**Grafik tidak muncul:**

- Pastikan ada data monitoring dalam 5 jam terakhir
- Jalankan sample data SQL script
- Cek console debug untuk error messages

**Tambak tidak muncul:**

- Pastikan user_id di tabel tambak sesuai dengan user yang login
- Verifikasi data di Supabase SQL Editor

### Build Issues

**Error: "Package not found"**

```bash
dotnet restore
dotnet build
```

**Error: "Target framework not installed"**

- Install .NET 8.0 SDK dari [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

## Konfigurasi Connection Modes

### Direct Connection (Port 5432)

- Direct ke database PostgreSQL
- Best untuk: Development, testing
- Pros: Stable, simple
- Cons: Limited concurrent connections

### Transaction Pooler (Port 6543)

- Connection pooling dengan transaction mode
- Best untuk: Production apps dengan banyak koneksi
- Pros: Scalable, efficient
- Cons: Beberapa PostgreSQL features tidak support

### Session Pooler (Port 5432)

- Connection pooling dengan session mode
- Best untuk: Long-running connections
- Pros: Support semua PostgreSQL features
- Cons: Limited concurrent connections

## Development Notes

### Architecture

- **MVVM Pattern:** Strict separation of concerns
- **Service Layer:** Business logic isolated dari UI
- **Data Binding:** Two-way binding untuk reactive UI
- **Async/Await:** Non-blocking UI operations

### Best Practices

- Password never stored in plain text (BCrypt hashing)
- Input validation di ViewModel layer
- Error handling dengan try-catch blocks
- Debug logging untuk troubleshooting
- Connection pooling untuk performance

### Dependencies

```xml
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc2" />
<PackageReference Include="Npgsql" Version="9.0.4" />
```

## Kontribusi

Untuk berkontribusi ke proyek ini:

1. Fork repository
2. Buat feature branch
3. Commit changes
4. Push ke branch
5. Buat Pull Request

## Lisensi

Proyek ini dibuat untuk keperluan pembelajaran dan development.

## Kontak & Support

Untuk pertanyaan atau issue:

- Buka issue di repository
- Contact project maintainer

## Changelog

### Version 1.0.0 (Current)

- Initial release
- Authentication system (Login/Register)
- Dashboard dengan real-time monitoring
- Grafik suhu, salinitas, pH
- Manajemen data panen
- Multi-tambak support
- Supabase PostgreSQL integration
