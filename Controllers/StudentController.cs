using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;        // Adjust namespace as needed
using backend.Models;
using backend.Dtos;        // Ensure DTOs are defined in this namespace
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
      private readonly PolarisDbContext _context;
      private readonly UserManager<ApplicationUser> _userManager;

      public StudentController(PolarisDbContext context, UserManager<ApplicationUser> userManager)
      {
          _context = context;
          _userManager = userManager;
      }

      // GET: api/student/sections/{sectionId}
      [HttpGet("sections/{sectionId}")]
      public async Task<ActionResult<StudentSectionDto>> GetSection(int sectionId)
      {
          var currentStudentId = GetCurrentStudentId();
          var currentSectionId = GetCurrentSectionId();
          
          // Verify the student is requesting their own section
          if (sectionId != currentSectionId)
          {
              return Forbid("You can only view your own section.");
          }

          var section = await _context.Sections
              .AsNoTracking()
              .Include(s => s.Instructor)
                  .ThenInclude(i => i!.User)
              .Include(s => s.AssessmentVisibilities.Where(av => av.IsVisible))
                  .ThenInclude(av => av.Assessment)
              .FirstOrDefaultAsync(s => s.SectionId == sectionId);

          if (section == null)
          {
              return NotFound("Section not found.");
          }

          var sectionDto = new StudentSectionDto
          {
              SectionId = section.SectionId,
              Instructor = section.Instructor == null ? null : new backend.Dtos.InstructorDto
              {
                  InstructorId = section.Instructor.InstructorId,
                  FullName = section.Instructor.User?.FullName
              },
              Assessments = section.AssessmentVisibilities.Select(av => new AssessmentDto
              {
                  AssessmentID = av.Assessment.AssessmentID,
                  Title = av.Assessment.Title,
                  Description = av.Assessment.Description
              }).ToList()
          };

          return Ok(sectionDto);
      }

      // GET: api/student/sections/current
      [HttpGet("sections/current")]
      [AllowAnonymous] // For debugging: to check if the issue is with authorization
      public async Task<ActionResult<StudentSectionDto>> GetCurrentStudentSection()
      {
          if (User.Identity?.IsAuthenticated == false)
          {
              return Unauthorized("User is not authenticated. Please provide a valid token to debug claims.");
          }

          var hasStudentRole = User.IsInRole("Student");
          if (!hasStudentRole)
          {
              // The user is authenticated but does not have the "Student" role.
              // This is the likely cause of the 403 Forbidden error.
              // Let's return the claims for debugging.
              var claims = User.Claims.Select(c => new { c.Type, c.Value });
              return new JsonResult(new { message = "User does not have 'Student' role.", claims }) { StatusCode = 403 };
          }
          
          var sectionId = GetCurrentSectionId();
          
          if (sectionId == 0)
          {
              return NotFound("You are not enrolled in any section.");
          }

          var section = await _context.Sections
              .AsNoTracking()
              .Include(s => s.Instructor)
                  .ThenInclude(i => i!.User)
              .Include(s => s.AssessmentVisibilities.Where(av => av.IsVisible))
                  .ThenInclude(av => av.Assessment)
              .FirstOrDefaultAsync(s => s.SectionId == sectionId);

          if (section == null)
          {
              return NotFound("Section not found.");
          }

          var sectionDto = new StudentSectionDto
          {
              SectionId = section.SectionId,
              Instructor = section.Instructor == null ? null : new backend.Dtos.InstructorDto
              {
                  InstructorId = section.Instructor.InstructorId,
                  FullName = section.Instructor.User?.FullName
              },
              Assessments = section.AssessmentVisibilities.Select(av => new AssessmentDto
              {
                  AssessmentID = av.Assessment.AssessmentID,
                  Title = av.Assessment.Title,
                  Description = av.Assessment.Description
              }).ToList()
          };

          return Ok(sectionDto);
      }

      // GET: api/student/profile
      [HttpGet("profile")]
      public async Task<ActionResult<StudentProfileDto>> GetProfile()
      {
          var studentId = GetCurrentStudentId();
          var matricNo = User.FindFirst("MatricNo")?.Value;
          var email = User.FindFirst(ClaimTypes.Email)?.Value;
          var fullName = User.FindFirst("FullName")?.Value;
          
          var student = await _context.Students
              .Include(s => s.Section)
              .FirstOrDefaultAsync(s => s.StudentId == studentId);

          if (student == null)
          {
              return NotFound("Student profile not found.");
          }

          return Ok(new StudentProfileDto
          {
              StudentId = studentId,
              MatricNo = matricNo ?? "",
              Email = email ?? "",
              FullName = fullName ?? "",
              SectionId = student.SectionId,
              SectionName = student.Section?.SectionId.ToString() ?? ""
          });
      }

      private int GetCurrentStudentId()
      {
          var studentIdClaim = User.FindFirst("StudentId")?.Value;
          return studentIdClaim != null ? int.Parse(studentIdClaim) : 0;
      }

      private int GetCurrentSectionId()
      {
          var sectionIdClaim = User.FindFirst("SectionId")?.Value;
          return !string.IsNullOrEmpty(sectionIdClaim) ? int.Parse(sectionIdClaim) : 0;
      }

      // GET: api/student
      // accessable by admin and instructor
      [HttpGet]
      [Authorize(Roles = "Student")]
      public async Task<IActionResult> GetStudents()
      {
        return Ok(await _context.Students.ToListAsync());
      }
    }

    public class StudentProfileDto
    {
        public int StudentId { get; set; }
        public string MatricNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty; 
    }
}
