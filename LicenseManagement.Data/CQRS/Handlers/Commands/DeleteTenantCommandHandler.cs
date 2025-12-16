using MediatR;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.CQRS.Commands;

namespace LicenseManagement.Data.CQRS.Handlers.Commands
{
    /// <summary>
    /// Handler for DeleteTenantCommand.
    /// Responsible for deleting tenants.
    /// </summary>
    public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, ApiResponse>
    {
        private readonly ITenantRepository _repository;

        public DeleteTenantCommandHandler(ITenantRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            if (request.TenantId <= 0)
                return ApiResponse.FailureResponse("Invalid tenant ID.");

            try
            {
                // Get existing tenant
                var existingTenant = await _repository.GetByIdAsync(request.TenantId);
                if (existingTenant == null)
                    return ApiResponse.FailureResponse($"Tenant with ID {request.TenantId} not found.");

                // Delete tenant
                await _repository.DeleteAsync(request.TenantId);

                return ApiResponse.SuccessResponse("Tenant deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse.FailureResponse($"Error deleting tenant: {ex.Message}");
            }
        }
    }
}