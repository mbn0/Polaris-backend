
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register ([FromBody] RegistrationDto registrationDto)
        {
          try 
          {
            if (!ModelState.IsValid)
            {
              return BadRequest(ModelState);
            }

            var appUser = new ApplicationUser
            {
              UserName = registrationDto.Email,
              Email = registrationDto.Email,
            };

            var createuser = await _userManager.CreateAsync(appUser, registrationDto.Password);

            if (createuser.Succeeded)
            {
              var roleResult = await _userManager.AddToRoleAsync(appUser, "Student");
              if(roleResult.Succeeded)
              {
                return Ok(new { message = "User created successfully" });
              }
              else
              {
                  return StatusCode(500, roleResult.Errors);
              }
            }
            else 
            {
              return StatusCode(500, createuser.Errors);
            }

          } catch (Exception ex) 
          {
            return BadRequest(new { message = ex.Message });

          }
        }
    }
}
