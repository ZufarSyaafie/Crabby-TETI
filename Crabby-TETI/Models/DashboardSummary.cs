namespace CrabbyTETI.Models
{
    // Dashboard summary statistics
    public class DashboardSummary
    {
        public int TotalTambak { get; set; }
        public decimal RataSuhu { get; set; }
        public decimal RataSalinitas { get; set; }
        public decimal RataPh { get; set; }
        public decimal TotalPanenBulanIni { get; set; }

        // Formatted properties for display
        public string TotalTambakFormatted => TotalTambak.ToString("N0");
        public string RataSuhuFormatted => $"{RataSuhu:F1}°C";
        public string RataSalinitasFormatted => $"{RataSalinitas:F1} ppt";
        public string RataPhFormatted => $"{RataPh:F1}";
        public string TotalPanenFormatted => $"{TotalPanenBulanIni:F1} kg";
    }
}
