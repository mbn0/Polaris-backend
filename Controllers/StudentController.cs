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

            var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);

            if (section == null)
            {
                return NotFound("Section not found.");
            }

            // Get instructor details separately
            var instructor = await _unitOfWork.Instructors.GetInstructorWithUserAsync(section.InstructorId);

            // Get all assessments (globally available)
            var allAssessments = await _unitOfWork.Assessments.GetAllAsync();
            
            // Get visibility settings for this section
            var visibilitySettings = await _unitOfWork.AssessmentVisibilities.GetBySectionIdAsync(sectionId);
            var visibilityDict = visibilitySettings.ToDictionary(av => av.AssessmentId, av => av.IsVisible);

            // Filter assessments that are visible (default to visible if no setting exists)
            var visibleAssessments = allAssessments
                .Where(assessment => visibilityDict.GetValueOrDefault(assessment.AssessmentID, true))
                .Select((assessment, index) => new StudentAssessmentVisibilityDto
                {
                    AssessmentVisibilityId = index + 1,
                    AssessmentId = assessment.AssessmentID,
                    SectionId = sectionId,
                    IsVisible = true, // Only showing visible ones
                    Assessment = new StudentAssessmentDto
                    {
                        AssessmentId = assessment.AssessmentID,
                        Title = assessment.Title,
                        Description = assessment.Description,
                        DueDate = DateTime.Now.AddDays(7), // Default since Assessment model doesn't have DueDate
                        MaxScore = 100 // Default since Assessment model doesn't have MaxScore
                    }
                }).ToList();

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
                AssessmentVisibilities = visibleAssessments
            };

            return Ok(sectionDto);
        }

        // GET: api/student/sections/current
        [HttpGet("sections/current")]
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
                    var claims = User.Claims.Select(c => new { c.Type, c.Value });
                    return new JsonResult(new { message = "User does not have 'Student' role.", claims }) { StatusCode = 403 };
                }
                
                var sectionId = GetCurrentSectionId();
                
                if (sectionId == 0)
                {
                    return NotFound("You are not enrolled in any section.");
                }

                var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);

                if (section == null)
                {
                    return NotFound("Section not found.");
                }

                // Get instructor details separately
                var instructor = await _unitOfWork.Instructors.GetInstructorWithUserAsync(section.InstructorId);

                // Get all assessments (globally available)
                var allAssessments = await _unitOfWork.Assessments.GetAllAsync();
                
                // Get visibility settings for this section
                var visibilitySettings = await _unitOfWork.AssessmentVisibilities.GetBySectionIdAsync(sectionId);
                var visibilityDict = visibilitySettings.ToDictionary(av => av.AssessmentId, av => av.IsVisible);

                // Filter assessments that are visible (default to visible if no setting exists)
                var visibleAssessments = allAssessments
                    .Where(assessment => visibilityDict.GetValueOrDefault(assessment.AssessmentID, true))
                    .Select((assessment, index) => new StudentAssessmentVisibilityDto
                    {
                        AssessmentVisibilityId = index + 1,
                        AssessmentId = assessment.AssessmentID,
                        SectionId = sectionId,
                        IsVisible = true, // Only showing visible ones
                        Assessment = new StudentAssessmentDto
                        {
                            AssessmentId = assessment.AssessmentID,
                            Title = assessment.Title,
                            Description = assessment.Description,
                            DueDate = DateTime.Now.AddDays(7), // Default since Assessment model doesn't have DueDate
                            MaxScore = 100 // Default since Assessment model doesn't have MaxScore
                        }
                    }).ToList();

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
                    AssessmentVisibilities = visibleAssessments
                };

                return Ok(sectionDto);
            }
            catch (Exception ex)
            {
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

        // POST: api/student/results
        [HttpPost("results")]
        public async Task<ActionResult<backend.Dtos.Assessment.ResultDto>> SubmitResult([FromBody] backend.Dtos.Assessment.SubmitResultDto submitResultDto)
        {
            try
            {
                var studentId = GetCurrentStudentId();
                if (studentId == 0)
                {
                    return BadRequest("Student ID not found in token.");
                }

                // Check if assessment exists and is visible to the student
                var sectionId = GetCurrentSectionId();
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(submitResultDto.AssessmentId);
                if (assessment == null)
                {
                    return NotFound("Assessment not found.");
                }

                // Check if assessment is visible for this section
                var visibility = await _unitOfWork.AssessmentVisibilities.GetBySectionAndAssessmentAsync(sectionId, submitResultDto.AssessmentId);
                if (visibility != null && !visibility.IsVisible)
                {
                    return Forbid("This assessment is not available for your section.");
                }

                // Check if student already has a result for this assessment
                var existingResult = await _unitOfWork.Results.GetResultByStudentAndAssessmentAsync(studentId, submitResultDto.AssessmentId);
                
                if (existingResult != null)
                {
                    // Update existing result (retake)
                    existingResult.Score = submitResultDto.Score;
                    existingResult.Date = submitResultDto.DateTaken;
                    
                    await _unitOfWork.Results.UpdateAsync(existingResult);
                    await _unitOfWork.SaveChangesAsync();

                    var updatedResultDto = new backend.Dtos.Assessment.ResultDto
                    {
                        ResultId = existingResult.ResultId,
                        StudentId = studentId,
                        AssessmentId = submitResultDto.AssessmentId,
                        AssessmentTitle = assessment.Title,
                        Score = existingResult.Score,
                        DateTaken = existingResult.Date,
                        StudentName = User.FindFirst("FullName")?.Value ?? "",
                        MatricNo = User.FindFirst("MatricNo")?.Value ?? ""
                    };

                    return Ok(updatedResultDto);
                }
                else
                {
                    // Create new result
                    var newResult = new Result
                    {
                        StudentId = studentId,
                        AssessmentId = submitResultDto.AssessmentId,
                        Score = submitResultDto.Score,
                        Date = submitResultDto.DateTaken
                    };

                    await _unitOfWork.Results.AddAsync(newResult);
                    await _unitOfWork.SaveChangesAsync();

                    var resultDto = new backend.Dtos.Assessment.ResultDto
                    {
                        ResultId = newResult.ResultId,
                        StudentId = studentId,
                        AssessmentId = submitResultDto.AssessmentId,
                        AssessmentTitle = assessment.Title,
                        Score = newResult.Score,
                        DateTaken = newResult.Date,
                        StudentName = User.FindFirst("FullName")?.Value ?? "",
                        MatricNo = User.FindFirst("MatricNo")?.Value ?? ""
                    };

                    return CreatedAtAction(nameof(GetResult), new { resultId = newResult.ResultId }, resultDto);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        // GET: api/student/results/{resultId}
        [HttpGet("results/{resultId}")]
        public async Task<ActionResult<backend.Dtos.Assessment.ResultDto>> GetResult(int resultId)
        {
            try
            {
                var studentId = GetCurrentStudentId();
                var result = await _unitOfWork.Results.GetByIdAsync(resultId);

                if (result == null)
                {
                    return NotFound("Result not found.");
                }

                // Ensure student can only access their own results
                if (result.StudentId != studentId)
                {
                    return Forbid("You can only access your own results.");
                }

                var assessment = await _unitOfWork.Assessments.GetByIdAsync(result.AssessmentId);

                var resultDto = new backend.Dtos.Assessment.ResultDto
                {
                    ResultId = result.ResultId,
                    StudentId = result.StudentId,
                    AssessmentId = result.AssessmentId,
                    AssessmentTitle = assessment?.Title ?? "",
                    Score = result.Score,
                    DateTaken = result.Date,
                    StudentName = User.FindFirst("FullName")?.Value ?? "",
                    MatricNo = User.FindFirst("MatricNo")?.Value ?? ""
                };

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        // GET: api/student/results
        [HttpGet("results")]
        public async Task<ActionResult<IEnumerable<backend.Dtos.Assessment.ResultDto>>> GetMyResults()
        {
            try
            {
                var studentId = GetCurrentStudentId();
                var results = await _unitOfWork.Results.GetResultsByStudentIdAsync(studentId);

                var resultDtos = results.Select(r => new backend.Dtos.Assessment.ResultDto
                {
                    ResultId = r.ResultId,
                    StudentId = r.StudentId,
                    AssessmentId = r.AssessmentId,
                    AssessmentTitle = r.Assessment?.Title ?? "",
                    Score = r.Score,
                    DateTaken = r.Date,
                    StudentName = User.FindFirst("FullName")?.Value ?? "",
                    MatricNo = User.FindFirst("MatricNo")?.Value ?? ""
                }).ToList();

                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        // POST: api/student/test-results (Temporary endpoint for testing)
        [HttpPost("test-results")]
        public async Task<ActionResult> CreateTestResults()
        {
            try
            {
                var studentId = GetCurrentStudentId();
                if (studentId == 0)
                {
                    return BadRequest("Student ID not found in token.");
                }

                // Get some assessments to create results for
                var assessments = await _unitOfWork.Assessments.GetAllAsync();
                var assessmentsList = assessments.Take(3).ToList();

                if (!assessmentsList.Any())
                {
                    return NotFound("No assessments found to create test results.");
                }

                // Check if results already exist for this student
                var existingResults = await _unitOfWork.Results.GetResultsByStudentIdAsync(studentId);
                if (existingResults.Any())
                {
                    return Ok(new { message = "Test results already exist for this student." });
                }

                // Create test results
                var testResults = new List<Result>();
                var random = new Random();
                
                for (int i = 0; i < assessmentsList.Count; i++)
                {
                    var score = 70 + random.Next(30); // Random score between 70-100
                    testResults.Add(new Result
                    {
                        StudentId = studentId,
                        AssessmentId = assessmentsList[i].AssessmentID,
                        Score = score,
                        Date = DateTime.Now.AddDays(-random.Next(30)) // Random date within last 30 days
                    });
                }

                foreach (var result in testResults)
                {
                    await _unitOfWork.Results.AddAsync(result);
                }
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { 
                    message = $"Created {testResults.Count} test results successfully.",
                    results = testResults.Select(r => new { 
                        AssessmentId = r.AssessmentId, 
                        Score = r.Score, 
                        Date = r.Date 
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error creating test results", 
                    error = ex.Message 
                });
            }
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
