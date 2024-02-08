using Azure;
using HouseCom.Models;
using Microsoft.AspNetCore.Identity;

namespace HouseCom.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        public AuthService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<bool> RegisterUser(LoginUser user)
        {
            var identityUser = new IdentityUser
            {
                UserName = user.UserName,
                Email = user.UserName
            };

            var result = await _userManager.CreateAsync(identityUser, user.Password);
            if (!result.Succeeded)
            {
                // Handle the error, either by throwing an exception or returning an error object.
               
                throw new ApplicationException($"User creation failed: {string.Join(", ", result.Errors.Select(x => "Code " + x.Code + " Description" + x.Description))}");
            }

            return true; // Or any other indication of success
        }

        public async Task<bool> Login(LoginUser user)
        {
            var identityUser = await _userManager.FindByEmailAsync(user.UserName);
            if (identityUser is null)
            {
                return false;
            }

            return await _userManager.CheckPasswordAsync(identityUser, user.Password);

            
        }
    }
}
