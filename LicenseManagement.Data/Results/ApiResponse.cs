using System;
using System.Collections.Generic;
using System.Text;

namespace LicenseManagement.Data.Results
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse SuccessResponse(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponse FailureResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new()
            };
        }
    }
}
