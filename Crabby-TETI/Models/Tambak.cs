using System;

namespace CrabbyTETI.Models
{
    /// <summary>
    /// Model untuk data Tambak
    /// </summary>
    public class Tambak
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public string Lokasi { get; set; } = string.Empty;
        public decimal LuasM2 { get; set; }
        public int Kapasitas { get; set; }
        public string Status { get; set; } = "Aktif";
        public int UserId { get; set; }
        
        // Data monitoring terkini (dari join query)
        public decimal? SuhuTerkini { get; set; }
        public decimal? SalinitasTerkini { get; set; }
        public decimal? PhTerkini { get; set; }
        public DateTime? LastUpdate { get; set; }
        
        // Data panen tahun ini (dari join query)
        public decimal TotalPanenTahunIni { get; set; }
        public decimal PendapatanTahunIni { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Property untuk UI display
        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Aktif" => "#4CAF50",
                    "Maintenance" => "#FF9800",
                    "Tidak Aktif" => "#F44336",
                    _ => "#9E9E9E"
                };
            }
        }

        public string LuasFormatted => $"{LuasM2:N0} m²";
        public string KapasitasFormatted => $"{Kapasitas:N0} ekor";
        public string TotalPanenFormatted => $"{TotalPanenTahunIni:N1} kg";
        public string PendapatanFormatted => $"Rp {PendapatanTahunIni:N0}";
    }
}
