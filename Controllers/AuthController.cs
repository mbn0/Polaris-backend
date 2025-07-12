using backend.Data;
using backend.Dtos.Auth;
using backend.Dtos.Student;
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
using backend.Repositories.Interfaces;
using System.Net.Mail;
using System.Net;

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
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
                var student = await _unitOfWork.Students.GetByUserIdAsync(user.Id);
                resp.MatricNo = student?.MatricNo;
                resp.SectionId = student?.SectionId;
            }
            // instructor-specific info
            else if (roles.Contains("Instructor"))
            {
                var instructor = await _unitOfWork.Instructors.GetByUserIdAsync(user.Id);
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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ModelState);
                }

                var existingUser = await _userManager.FindByEmailAsync(registrationDto.Email);
                if (existingUser != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { message = "A user with this email already exists." });
                }

                var appUser = new ApplicationUser
                {
                    UserName = registrationDto.Email,
                    Email = registrationDto.Email,
                    FullName = registrationDto.FullName
                };

                var createUserResult = await _userManager.CreateAsync(appUser, registrationDto.Password);

                if (createUserResult.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "Student");
                    if (!roleResult.Succeeded)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return StatusCode(500, createUserResult.Errors);
                }

                var student = new Student
                {
                    MatricNo = registrationDto.MatricNo,
                    UserId = appUser.Id
                };

                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

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
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        // Password Reset Endpoints
        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal if user exists or not for security
                    return Ok(new { message = "If an account with this email exists, a password reset OTP has been sent." });
                }

                // Generate 6-digit OTP
                var random = new Random();
                var otp = random.Next(100000, 999999).ToString();
                
                // Store OTP and expiry (5 minutes)
                user.PasswordResetOtp = otp;
                user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);
                
                await _userManager.UpdateAsync(user);

                // Send OTP via email
                await SendPasswordResetEmail(user.Email!, user.FullName, otp);

                return Ok(new { message = "If an account with this email exists, a password reset OTP has been sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Invalid email or OTP." });
                }

                // Check OTP validity
                if (user.PasswordResetOtp != request.Otp || 
                    user.PasswordResetOtpExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Invalid or expired OTP." });
                }

                return Ok(new { message = "OTP verified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while verifying OTP." });
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Invalid email or OTP." });
                }

                // Validate OTP
                if (user.PasswordResetOtp != request.Otp || 
                    user.PasswordResetOtpExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Invalid or expired OTP." });
                }

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                
                if (result.Succeeded)
                {
                    // Clear OTP fields
                    user.PasswordResetOtp = null;
                    user.PasswordResetOtpExpiry = null;
                    await _userManager.UpdateAsync(user);

                    return Ok(new { message = "Password reset successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to reset password.", errors = result.Errors });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while resetting password." });
            }
        }

        private async Task SendPasswordResetEmail(string email, string fullName, string otp)
        {
            try
            {
                // Get email settings from configuration
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "live.smtp.mailtrap.io";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "api";
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "3b777591f83a047e2f6195eee833657e";
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "hello@jibna.live";
                var fromName = _configuration["EmailSettings:FromName"] ?? "Polaris Learning System";

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    IsBodyHtml = true,
                    Subject = "🔐 Your Password Reset OTP",
                    Body = $@"
                      <div style='font-family:Arial,sans-serif;'>
                        <h2 style='color:#2E8B57;'>Password Reset Request</h2>
                        <p>
                          Dear <b>{fullName ?? email}</b>,<br/><br/>
                          Your One-Time Password (OTP) for password reset is:<br/>
                          <span style='font-size:1.5em;color:#2E8B57;font-weight:bold;'>{otp}</span><br/><br/>
                          This code will expire in 5 minutes.<br/><br/>
                          <em>If you did not request this, please ignore this email.</em>
                        </p>
                      </div>"
                };
                message.To.Add(email);

                using var smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };
                
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log the error but don't expose it to the client
                Console.WriteLine($"Failed to send email: {ex.Message}");
                // In production, you might want to use a proper logging service
            }
        }
    }
}
