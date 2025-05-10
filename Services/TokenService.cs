
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services
{
    public class ToeknService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PolarisDbContext _context;

        public ToeknService(IConfiguration config, UserManager<ApplicationUser> userManager, PolarisDbContext context)
        {

            _config = config;
            _userManager = userManager;
            _context = context;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
        }

        public async Task<string> CreateToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim("FullName", user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };

            // a user might have multiple roles
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                if (role == "Student")
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    if (student != null)
                    {
                        claims.Add(new Claim("StudentId", student.StudentId.ToString()));
                        claims.Add(new Claim("MatricNo", student.MatricNo));
                        claims.Add(new Claim("SectionId", student.SectionId.ToString() ?? ""));
                    }
                }
                else if (role == "Instructor")
                {
                    var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                    if (instructor != null)
                    {
                        claims.Add(new Claim("InstructorId", instructor.InstructorId.ToString()));
                    }
                }
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // return the token as a string
            return tokenHandler.WriteToken(token);
        }
    }
}

