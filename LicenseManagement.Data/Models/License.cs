namespace LicenseManagement.Data.Models
{
    public class License
    {
        public int LicenseID { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}