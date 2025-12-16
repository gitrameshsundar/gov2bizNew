using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.CQRS.Queries;

namespace LicenseManagement.Data.CQRS.Handlers.Queries
{
    /// <summary>
    /// Handler for GetTenantByIdQuery.
    /// Responsible for retrieving a specific tenant.
    /// </summary>
    public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, ApiResult<Tenant>>
    {
        private readonly ITenantRepository _repository;

        public GetTenantByIdQueryHandler(ITenantRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResult<Tenant>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
        {
            // Validate query
            if (request.TenantId <= 0)
                return ApiResult<Tenant>.FailureResult("Invalid tenant ID.");

            try
            {
                var tenant = await _repository.GetByIdAsync(request.TenantId);
                if (tenant == null)
                    return ApiResult<Tenant>.FailureResult($"Tenant with ID {request.TenantId} not found.");

                return ApiResult<Tenant>.SuccessResult(
                    tenant,
                    "Tenant retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResult<Tenant>.FailureResult($"Error retrieving tenant: {ex.Message}");
            }
        }
    }
}