using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;        // Adjust namespace as needed
using backend.Models;
using backend.Dtos.Student;        // Student DTOs
using backend.Dtos.Instructor;     // Instructor DTOs  
using backend.Dtos.Assessment;     // Assessment DTOs
using System.Security.Claims;
using backend.Repositories.Interfaces;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
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

            var section = await _unitOfWork.Sections.GetSectionWithAssessmentVisibilitiesAsync(sectionId);

            if (section == null)
            {
                return NotFound("Section not found.");
            }

            // Get instructor details separately
            var instructor = await _unitOfWork.Instructors.GetInstructorWithUserAsync(section.InstructorId);

            var sectionDto = new StudentSectionDto
            {
                SectionId = section.SectionId,
                Instructor = instructor == null ? null : new StudentInstructorDto
                {
                    InstructorId = instructor.InstructorId,
                    UserId = instructor.UserId,
                    User = new StudentUserDto
                    {
                        Id = instructor.UserId,
                        FullName = instructor.User?.FullName ?? "",
                        Email = instructor.User?.Email ?? ""
                    }
                },
                AssessmentVisibilities = section.AssessmentVisibilities?
                    .Where(av => av.IsVisible && av.Assessment != null)
                    .Select((av, index) => new StudentAssessmentVisibilityDto
                    {
                        AssessmentVisibilityId = index + 1, // Since there's no ID in the model, use index
                        AssessmentId = av.AssessmentId,
                        SectionId = av.SectionId,
                        IsVisible = av.IsVisible,
                        Assessment = new StudentAssessmentDto
                        {
                            AssessmentId = av.Assessment.AssessmentID,
                            Title = av.Assessment.Title,
                            Description = av.Assessment.Description,
                            DueDate = DateTime.Now.AddDays(7), // Default since Assessment model doesn't have DueDate
                            MaxScore = 100 // Default since Assessment model doesn't have MaxScore
                        }
                    }).ToList() ?? new List<StudentAssessmentVisibilityDto>()
            };

            return Ok(sectionDto);
        }

        // GET: api/student/sections/current
        [HttpGet("sections/current")] // For debugging: to check if the issue is with authorization
        public async Task<ActionResult<StudentSectionDto>> GetCurrentStudentSection()
        {
            try
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
                
                if (sectionId == 0)
                {
                    return NotFound("You are not enrolled in any section.");
                }

                var section = await _unitOfWork.Sections.GetSectionWithAssessmentVisibilitiesAsync(sectionId);

                if (section == null)
                {
                    return NotFound("Section not found.");
                }

                // Get instructor details separately
                var instructor = await _unitOfWork.Instructors.GetInstructorWithUserAsync(section.InstructorId);

                var sectionDto = new StudentSectionDto
                {
                    SectionId = section.SectionId,
                    Instructor = instructor == null ? null : new StudentInstructorDto
                    {
                        InstructorId = instructor.InstructorId,
                        UserId = instructor.UserId,
                        User = new StudentUserDto
                        {
                            Id = instructor.UserId,
                            FullName = instructor.User?.FullName ?? "",
                            Email = instructor.User?.Email ?? ""
                        }
                    },
                    AssessmentVisibilities = section.AssessmentVisibilities?
                        .Where(av => av.IsVisible && av.Assessment != null)
                        .Select((av, index) => new StudentAssessmentVisibilityDto
                        {
                            AssessmentVisibilityId = index + 1, // Since there's no ID in the model, use index
                            AssessmentId = av.AssessmentId,
                            SectionId = av.SectionId,
                            IsVisible = av.IsVisible,
                            Assessment = new StudentAssessmentDto
                            {
                                AssessmentId = av.Assessment.AssessmentID,
                                Title = av.Assessment.Title,
                                Description = av.Assessment.Description,
                                DueDate = DateTime.Now.AddDays(7), // Default since Assessment model doesn't have DueDate
                                MaxScore = 100 // Default since Assessment model doesn't have MaxScore
                            }
                        }).ToList() ?? new List<StudentAssessmentVisibilityDto>()
                };

                return Ok(sectionDto);
            }
            catch (Exception ex)
            {
                // Return detailed error information for debugging
                return StatusCode(500, new { 
                    message = "Internal server error", 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // GET: api/student/profile
        [HttpGet("profile")]
        public async Task<ActionResult<StudentProfileDto>> GetProfile()
        {
            var studentId = GetCurrentStudentId();
            var matricNo = User.FindFirst("MatricNo")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;
            
            var student = await _unitOfWork.Students.GetStudentWithSectionAsync(studentId);

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
            if (string.IsNullOrWhiteSpace(sectionIdClaim)) return 0;
            if (int.TryParse(sectionIdClaim, out var secId)) return secId;
            return 0;
        }

        // GET: api/student
        // accessable by admin and instructor
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            return Ok(students);
        }
    }
}
