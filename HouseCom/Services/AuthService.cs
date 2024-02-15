using Azure;
using HouseCom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HouseCom.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IConfiguration _config;

        public AuthService(UserManager<IdentityUser> userManager, IConfiguration config, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _config = config;
            _roleManager = roleManager;
            this.secretKey = config.GetValue<string>("Jwt:Key"); ;
        }
        public async Task<bool> RegisterUser(LoginUser user)
        {
            var identityUser = new ApplicationUser
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

            if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole("admin"));
                await _roleManager.CreateAsync(new IdentityRole("customer"));
            }
            await _userManager.AddToRoleAsync(identityUser, "admin");
           

            return true; // Or any other indication of success
        }

        public async Task<string> Login(LoginUser user)
        {
            var identityUser = await _userManager.FindByEmailAsync(user.UserName);

            var isValid = await _userManager.CheckPasswordAsync(identityUser, user.Password);
            if (identityUser is null || isValid == false)
            {

                return "";
            }
            //if user was found generate JWT Token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(identityUser);
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes(secretKey);

            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(new Claim[]
            //    {
            //        new Claim(ClaimTypes.Name, user.UserName.ToString()),
            //        new Claim(ClaimTypes.Role, roles.FirstOrDefault())
            //    }),
            //    Expires = DateTime.UtcNow.AddDays(7),
            //    SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //};


            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName.ToString()),
            new Claim(ClaimTypes.Role, roles.FirstOrDefault())
        };
            var token = new JwtSecurityToken(
            issuer: _config.GetSection("Jwt:Issuer").Value,     
            audience: _config.GetSection("Jwt:Audience").Value,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(10), // Set the token expiration time
            signingCredentials: credentials
        );

            var tokenHandler = new JwtSecurityTokenHandler();
           
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;



        }

        

    }   
}
