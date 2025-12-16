using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;

namespace LicenseManagement.Data.CQRS.Queries
{
    /// <summary>
    /// Query to retrieve a specific tenant by ID.
    /// </summary>
    public class GetTenantByIdQuery : IRequest<ApiResult<Tenant>>
    {
        public int TenantId { get; set; }

        public GetTenantByIdQuery(int tenantId)
        {
            TenantId = tenantId;
        }
    }
}