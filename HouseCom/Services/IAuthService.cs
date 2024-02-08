using HouseCom.Models;

namespace HouseCom.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginUser user);
        Task<bool> RegisterUser(LoginUser user);
    }
}