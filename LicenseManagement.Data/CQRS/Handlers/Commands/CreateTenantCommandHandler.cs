using MediatR;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.CQRS.Commands;

namespace LicenseManagement.Data.CQRS.Handlers.Commands
{
    /// <summary>
    /// Handler for CreateTenantCommand.
    /// Responsible for creating new tenants.
    /// </summary>
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, ApiResult<Tenant>>
    {
        private readonly ITenantRepository _repository;

        public CreateTenantCommandHandler(ITenantRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResult<Tenant>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResult<Tenant>.FailureResult("Tenant name is required.");

            try
            {
                // Create tenant entity
                var tenant = new Tenant
                {
                    Name = request.Name,
                    //ContactEmail = request.ContactEmail,
                    //PhoneNumber = request.PhoneNumber,
                    //Address = request.Address,
                    //CreatedDate = DateTime.UtcNow
                };

                // Persist to database
                var createdTenant = await _repository.CreateAsync(tenant);

                return ApiResult<Tenant>.SuccessResult(
                    createdTenant,
                    "Tenant created successfully");
            }
            catch (Exception ex)
            {
                return ApiResult<Tenant>.FailureResult($"Error creating tenant: {ex.Message}");
            }
        }
    }
}