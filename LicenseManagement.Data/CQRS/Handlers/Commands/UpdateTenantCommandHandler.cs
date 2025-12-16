using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.CQRS.Commands;

namespace LicenseManagement.Data.CQRS.Handlers.Commands
{
    /// <summary>
    /// Handler for UpdateTenantCommand.
    /// Responsible for updating existing tenants.
    /// </summary>
    public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, ApiResult<Tenant>>
    {
        private readonly ITenantRepository _repository;

        public UpdateTenantCommandHandler(ITenantRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResult<Tenant>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            if (request.TenantId <= 0)
                return ApiResult<Tenant>.FailureResult("Invalid tenant ID.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResult<Tenant>.FailureResult("Tenant name is required.");

            try
            {
                // Get existing tenant
                var existingTenant = await _repository.GetByIdAsync(request.TenantId);
                if (existingTenant == null)
                    return ApiResult<Tenant>.FailureResult($"Tenant with ID {request.TenantId} not found.");

                // Update tenant properties
                existingTenant.Name = request.Name;
                //existingTenant.ContactEmail = request.ContactEmail;
                //existingTenant.PhoneNumber = request.PhoneNumber;
                //existingTenant.Address = request.Address;
                //existingTenant.UpdatedDate = DateTime.UtcNow;

                // Persist changes
                var updatedTenant = await _repository.UpdateAsync(request.TenantId, existingTenant);

                return ApiResult<Tenant>.SuccessResult(
                    updatedTenant,
                    "Tenant updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResult<Tenant>.FailureResult($"Error updating tenant: {ex.Message}");
            }
        }
    }
}