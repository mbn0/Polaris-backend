using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly PolarisDbContext      _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService         _tokenService;

        public AdminController(
            PolarisDbContext context,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _context      = context;
            _userManager  = userManager;
            _tokenService = tokenService;
        }

        // ----------------------------------------------------------------
        // SECTION MANAGEMENT
        // ----------------------------------------------------------------

        [HttpGet("sections")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSections()
        {
            var sections = await _context.Sections
                .Include(s => s.Instructor).ThenInclude(i => i!.User)
                .Include(s => s.Students).ThenInclude(st => st!.User)
                .ToListAsync();

            return sections.Select(s => new SectionDto
            {
                SectionId = s.SectionId,
                InstructorUserId = s.Instructor?.UserId ?? "",
                InstructorName = s.Instructor?.User?.FullName ?? "",
                Students = s.Students?.Select(st => new StudentBriefDto
                {
                    UserId = st.UserId,
                    FullName = st.User?.FullName ?? "",
                    MatricNo = st.MatricNo
                }).ToList() ?? new List<StudentBriefDto>()
            }).ToList();
        }

        [HttpGet("sections/{id}")]
        public async Task<ActionResult<SectionDto>> GetSection(int id)
        {
            var s = await _context.Sections
                .Include(x => x.Instructor).ThenInclude(i => i!.User)
                .Include(x => x.Students).ThenInclude(st => st!.User)
                .FirstOrDefaultAsync(x => x.SectionId == id);

            if (s == null) return NotFound();
            return new SectionDto
            {
                SectionId = s.SectionId,
                InstructorUserId = s.Instructor?.UserId ?? "",
                InstructorName = s.Instructor?.User?.FullName ?? "",
                Students = s.Students?.Select(st => new StudentBriefDto
                {
                    UserId = st.UserId,
                    FullName = st.User?.FullName ?? "",
                    MatricNo = st.MatricNo
                }).ToList() ?? new List<StudentBriefDto>()
            };
        }

        [HttpPost("sections")]
        public async Task<ActionResult<CreateSectionResponseDto>> CreateSection([FromBody] CreateSectionDto dto)
        {
            // Verify instructor exists
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == dto.InstructorUserId);
            if (instructor == null)
                return BadRequest("Instructor not found");

            var sec = new Section { InstructorId = instructor.InstructorId };
            _context.Sections.Add(sec);
            await _context.SaveChangesAsync();

            return new CreateSectionResponseDto
            {
                Id = sec.SectionId,
                InstructorId = instructor.InstructorId,
                InstructorUserId = instructor.UserId
            };
        }

        [HttpPut("sections/{id}")]
        public async Task<ActionResult> UpdateSection(int id, [FromBody] UpdateSectionDto dto)
        {
            var sec = await _context.Sections.FindAsync(id);
            if (sec == null) return NotFound();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == dto.InstructorUserId);
            if (instructor == null)
                return BadRequest("Instructor not found");

            sec.InstructorId = instructor.InstructorId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("sections/{id}")]
        public async Task<ActionResult> DeleteSection(int id)
        {
            var sec = await _context.Sections.FindAsync(id);
            if (sec == null) return NotFound();
            _context.Sections.Remove(sec);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("sections/{sectionId}/students/{userId}")]
        public async Task<ActionResult> AddStudentToSection(int sectionId, string userId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound("Student not found");
            student.SectionId = sectionId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("sections/{sectionId}/students/{userId}")]
        public async Task<ActionResult> RemoveStudentFromSection(int sectionId, string userId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(st => st.UserId == userId && st.SectionId == sectionId);
            if (student == null) return NotFound();
            student.SectionId = null;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ----------------------------------------------------------------
        // USER MANAGEMENT
        // ----------------------------------------------------------------

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var list  = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserDto
                {
                    Id       = u.Id,
                    Email    = u.Email!,
                    FullName = u.FullName,
                    Roles    = roles
                });
            }
            return list;
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(u);
            return new UserDto
            {
                Id       = u.Id,
                Email    = u.Email!,
                FullName = u.FullName,
                Roles    = roles
            };
        }

        [HttpGet("instructors")]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructors()
        {
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .ToListAsync();

            return instructors.Select(i => new InstructorDto
            {
                Id = i.InstructorId,
                UserId = i.UserId,
                Name = i.User?.FullName ?? "",
                Email = i.User?.Email ?? "",
                SectionsCount = i.Sections?.Count ?? 0
            }).ToList();
        }

        [HttpPost("users")]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var u = new ApplicationUser
            {
                UserName = dto.Email,
                Email    = dto.Email,
                FullName = dto.FullName
            };
            var createRes = await _userManager.CreateAsync(u, dto.Password);
            if (!createRes.Succeeded) return BadRequest(createRes.Errors);

            // assign roles
            foreach (var role in dto.Roles)
                await _userManager.AddToRoleAsync(u, role);

            // if student, create profile
            if (dto.Roles.Contains("Student"))
            {
                _context.Students.Add(new Student
                {
                    UserId  = u.Id,
                    MatricNo = dto.MatricNo!,
                    SectionId = dto.SectionId
                });
            }
            // if instructor, create profile
            if (dto.Roles.Contains("Instructor"))
            {
                _context.Instructors.Add(new Instructor
                {
                    UserId = u.Id
                });
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = u.Id }, null);
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            u.Email    = dto.Email;
            u.UserName = dto.Email;
            u.FullName = dto.FullName;
            var pwdRes = await _userManager.RemovePasswordAsync(u);
            if (dto.Password is not null)
                await _userManager.AddPasswordAsync(u, dto.Password);

            var currentRoles = await _userManager.GetRolesAsync(u);
            // remove outdated roles
            await _userManager.RemoveFromRolesAsync(u, currentRoles.Except(dto.Roles));
            // add new roles
            await _userManager.AddToRolesAsync(u, dto.Roles.Except(currentRoles));

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();
            var delRes = await _userManager.DeleteAsync(u);
            if (!delRes.Succeeded) return StatusCode(500, delRes.Errors);
            return NoContent();
        }
    }

    #region DTOs
    public class SectionDto
    {
        public int SectionId { get; set; }
        public string InstructorUserId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public List<StudentBriefDto> Students { get; set; } = new();
    }

    public class StudentBriefDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MatricNo { get; set; } = string.Empty;
    }

    public class CreateSectionResponseDto
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public string InstructorUserId { get; set; } = string.Empty;
    }

    public class CreateSectionDto
    {
        public string InstructorUserId { get; set; } = string.Empty;
    }

    public class UpdateSectionDto
    {
        public string InstructorUserId { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id       { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class CreateUserDto
    {
        public string Email     { get; set; } = string.Empty;
        public string FullName  { get; set; } = string.Empty;
        public string Password  { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string? MatricNo   { get; set; }
        public int? SectionId     { get; set; }
    }

    public class UpdateUserDto
    {
        public string Email     { get; set; } = string.Empty;
        public string FullName  { get; set; } = string.Empty;
        public string? Password { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class InstructorDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int SectionsCount { get; set; }
    }
    #endregion
}

