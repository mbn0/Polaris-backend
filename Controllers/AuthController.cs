
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
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
        private readonly PolarisDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, PolarisDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationDto registrationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _userManager.FindByEmailAsync(registrationDto.Email);
                if (existingUser != null)
                    return BadRequest("A user with this email already exists.");

                var appUser = new ApplicationUser
                {
                    UserName = registrationDto.Email,
                    Email = registrationDto.Email,
                };

                var createUserResult = await _userManager.CreateAsync(appUser, registrationDto.Password);

                if (createUserResult.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "Student");
                    if (!roleResult.Succeeded)
                    { return StatusCode(500, roleResult.Errors); }
                }
                else
                { return StatusCode(500, createUserResult.Errors); }

                var student = new Student
                  {
                    MatricNo = registrationDto.MatricNo,
                    UserId = appUser.Id 
                  };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return Ok(new NewStudentDto
                {
                    FullName = appUser.FullName,
                    Email = appUser.Email,
                    MatricNo = student.MatricNo,
                    Token = await _tokenService.CreateToken(appUser),
                    SectionId = student.SectionId ?? 0
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
