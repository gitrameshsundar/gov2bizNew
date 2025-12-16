using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;

namespace LicenseManagement.Data.CQRS.Commands
{
    /// <summary>
    /// Command to update an existing tenant.
    /// </summary>
    public class UpdateTenantCommand : IRequest<ApiResult<Tenant>>
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}