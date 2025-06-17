
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{
    public class AdminRegistrationDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.Users
                          .FirstOrDefaultAsync(u => u.UserName == loginDto.Email.ToLower());
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var pwCheck = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!pwCheck.Succeeded)
                return Unauthorized(new { message = "Invalid username or password" });

            // build base response
            var roles = await _userManager.GetRolesAsync(user);
            var resp = new AuthResponseDto
            {
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles,
                Token = await _tokenService.CreateToken(user)
            };

            // student-specific info
            if (roles.Contains("Student"))
            {
                var student = await _context.Students
                                   .FirstOrDefaultAsync(s => s.UserId == user.Id);
                resp.MatricNo = student?.MatricNo;
                resp.SectionId = student?.SectionId;
            }
            // instructor-specific info
            else if (roles.Contains("Instructor"))
            {
                var instructor = await _context.Instructors
                                       .FirstOrDefaultAsync(i => i.UserId == user.Id);
                // e.g. resp.InstructorId = instructor?.InstructorId;
                // add whatever else you need for instructors
            }

            return Ok(resp);
        }

        [HttpPost("admin-register")]
        public async Task<IActionResult> AdminRegister([FromBody] AdminRegistrationDto registrationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _userManager.FindByEmailAsync(registrationDto.Email);
                if (existingUser != null)
                    return BadRequest(new { message = "A user with this email already exists." });

                var appUser = new ApplicationUser
                {
                    UserName = registrationDto.Email,
                    Email = registrationDto.Email,
                };

                var createUserResult = await _userManager.CreateAsync(appUser, registrationDto.Password);

                if (createUserResult.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "Admin");
                    if (!roleResult.Succeeded)
                    { return StatusCode(500, roleResult.Errors); }

                }
                else
                { return StatusCode(500, createUserResult.Errors); }

                var token = await _tokenService.CreateToken(appUser);
                return Ok(new
                {
                    Email = appUser.Email,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //admin-login

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
                    return BadRequest(new { message = "A user with this email already exists." });

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
