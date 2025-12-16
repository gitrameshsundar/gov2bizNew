using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;

namespace LicenseManagement.Data.CQRS.Queries
{
    /// <summary>
    /// Query to retrieve all tenants.
    /// Queries do not modify state and return data.
    /// </summary>
    public class GetAllTenantsQuery : IRequest<ApiResult<List<Tenant>>>
    {
    }
}