namespace LicenseManagement.DTO
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public int TenantID { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
