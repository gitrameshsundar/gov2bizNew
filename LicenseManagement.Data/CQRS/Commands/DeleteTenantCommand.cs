using MediatR;
using LicenseManagement.Data.Results;

namespace LicenseManagement.Data.CQRS.Commands
{
    /// <summary>
    /// Command to delete a tenant.
    /// </summary>
    public class DeleteTenantCommand : IRequest<ApiResponse>
    {
        public int TenantId { get; set; }

        public DeleteTenantCommand(int tenantId)
        {
            TenantId = tenantId;
        }
    }
}