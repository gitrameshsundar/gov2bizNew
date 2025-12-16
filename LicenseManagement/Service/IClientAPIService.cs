using LicenseManagement.DTO;

namespace LicenseManagement.Service
{
    public interface IClientAPIService
    {
        Task<UserDto?> LoginAsync(LoginView loginInput);
    }
}
