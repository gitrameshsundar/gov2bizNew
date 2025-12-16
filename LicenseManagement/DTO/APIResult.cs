namespace LicenseManagement.DTO
{
    public class ApiResult<T>
    {
        public T Data { get; set; } = default!;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
