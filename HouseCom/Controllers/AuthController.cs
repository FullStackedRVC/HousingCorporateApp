using HouseCom.Models;
using HouseCom.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HouseCom.Controllers
{
    
    [ApiController]
    [ApiVersion("1")]
    [Route("api/authorizations")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(LoginUser user)
        {
            if(await _authService.RegisterUser(user))
            {
                return Ok("Successfully done");
            }
            return BadRequest("Something went wrong");

        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUser user)
        {
            
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }
           
                var tokenString = await _authService.Login(user);
            if (string.IsNullOrEmpty( tokenString))
            {
                return BadRequest();
            }
                return Ok(tokenString);           
           
        }
    }
}
