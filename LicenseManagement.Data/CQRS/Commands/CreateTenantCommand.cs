using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;

namespace LicenseManagement.Data.CQRS.Commands
{
    /// <summary>
    /// Command to create a new tenant.
    /// Commands modify state and return a response.
    /// </summary>
    public class CreateTenantCommand : IRequest<ApiResult<Tenant>>
    {
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}