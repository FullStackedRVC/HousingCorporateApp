using HouseCom.Models;

namespace HouseCom.Services
{
    public interface IAuthService
    {
       
        Task<string> Login(LoginUser user);
        Task<bool> RegisterUser(LoginUser user);
    }
}