namespace LicenseManagement.Data.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Unread";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}