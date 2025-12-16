using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.CQRS.Queries;

namespace LicenseManagement.Data.CQRS.Handlers.Queries
{
    /// <summary>
    /// Handler for GetAllTenantsQuery.
    /// Responsible for retrieving all tenants.
    /// </summary>
    public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, ApiResult<List<Tenant>>>
    {
        private readonly ITenantRepository _repository;

        public GetAllTenantsQueryHandler(ITenantRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResult<List<Tenant>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tenants = await _repository.GetAllAsync();

                return ApiResult<List<Tenant>>.SuccessResult(
                    tenants,
                    "Tenants retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResult<List<Tenant>>.FailureResult($"Error retrieving tenants: {ex.Message}");
            }
        }
    }
}