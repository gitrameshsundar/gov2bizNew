namespace LicenseManagement.DTO
{
    public class NotificationDto
    {
        public int NotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Unread";
        public DateTime CreatedDate { get; set; }
    }
}
