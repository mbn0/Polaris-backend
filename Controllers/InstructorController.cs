using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using backend.Data;
using backend.Dtos.Common;
using backend.Dtos.Instructor;
using backend.Repositories.Interfaces;
using System.Collections.Generic;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Instructor, Admin")]
    public class InstructorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public InstructorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Get all sections assigned to the instructor
        [HttpGet("sections")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InstructorSectionDto>>>> GetInstructorSections()
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var sections = await _unitOfWork.Sections.GetSectionsByInstructorIdAsync(instructorId);

                var sectionDtos = sections.Select(section => new InstructorSectionDto
                {
                    SectionId = section.SectionId,
                    InstructorId = section.InstructorId,
                    Students = section.Students?.Select(student => new StudentBriefDto
                    {
                        StudentId = student.StudentId,
                        UserId = student.UserId,
                        FullName = student.User?.FullName ?? "",
                        MatricNo = student.MatricNo
                    }).ToList() ?? new List<StudentBriefDto>(),
                    AssessmentVisibilities = section.AssessmentVisibilities?.Select(av => new AssessmentVisibilityDto
                    {
                        AssessmentId = av.AssessmentId,
                        AssessmentTitle = av.Assessment?.Title ?? "",
                        AssessmentDescription = av.Assessment?.Description ?? "",
                        IsVisible = av.IsVisible
                    }).ToList() ?? new List<AssessmentVisibilityDto>()
                }).ToList();

                return Ok(ApiResponse<IEnumerable<InstructorSectionDto>>.SuccessResponse(sectionDtos, "Sections retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<InstructorSectionDto>>.ErrorResponse("Failed to retrieve sections", ex.Message));
            }
        }

        // Get detailed section information including students and their results
        [HttpGet("sections/{sectionId}")]
        public async Task<ActionResult<ApiResponse<InstructorSectionDto>>> GetSectionDetails(int sectionId)
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var section = await _unitOfWork.Sections.GetSectionForInstructorAsync(sectionId, instructorId);

                if (section == null)
                {
                    return NotFound(ApiResponse<InstructorSectionDto>.ErrorResponse("Section not found or you don't have access to it."));
                }

                var sectionDto = new InstructorSectionDto
                {
                    SectionId = section.SectionId,
                    InstructorId = section.InstructorId,
                    Students = section.Students?.Select(student => new StudentBriefDto
                    {
                        StudentId = student.StudentId,
                        UserId = student.UserId,
                        FullName = student.User?.FullName ?? "",
                        MatricNo = student.MatricNo
                    }).ToList() ?? new List<StudentBriefDto>(),
                    AssessmentVisibilities = section.AssessmentVisibilities?.Select(av => new AssessmentVisibilityDto
                    {
                        AssessmentId = av.AssessmentId,
                        AssessmentTitle = av.Assessment?.Title ?? "",
                        AssessmentDescription = av.Assessment?.Description ?? "",
                        IsVisible = av.IsVisible
                    }).ToList() ?? new List<AssessmentVisibilityDto>()
                };

                return Ok(ApiResponse<InstructorSectionDto>.SuccessResponse(sectionDto, "Section details retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InstructorSectionDto>.ErrorResponse("Failed to retrieve section details", ex.Message));
            }
        }

        // Get student results for a specific section
        [HttpGet("sections/{sectionId}/results")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentResultDto>>>> GetSectionResults(int sectionId)
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var section = await _unitOfWork.Sections.GetSectionWithStudentsAndResultsAsync(sectionId);

                if (section == null || section.InstructorId != instructorId)
                {
                    return NotFound(ApiResponse<IEnumerable<StudentResultDto>>.ErrorResponse("Section not found or you don't have access to it."));
                }

                var studentResults = section.Students?
                    .Select(student => new StudentResultDto
                    {
                        StudentId = student.StudentId,
                        MatricNo = student.MatricNo,
                        FullName = student.User?.FullName ?? "",
                        Results = student.Results?
                            .Select(r => new InstructorResultDto
                            {
                                AssessmentId = r.Assessment?.AssessmentID ?? 0,
                                AssessmentTitle = r.Assessment?.Title ?? "",
                                Score = r.Score,
                                DateTaken = r.Date
                            })
                            .ToList() ?? new List<InstructorResultDto>()
                    })
                    .ToList() ?? new List<StudentResultDto>();

// Debug logging (can be removed in production)
                Console.WriteLine($"Debug: Found {studentResults.Count} students");
                foreach (var student in studentResults)
                {
                    Console.WriteLine($"Debug: Student {student.FullName} ({student.MatricNo}) has {student.Results.Count} results");
                    foreach (var result in student.Results)
                    {
                        Console.WriteLine($"Debug: - Assessment {result.AssessmentTitle}: Score {result.Score}");
                    }
                }

                return Ok(ApiResponse<IEnumerable<StudentResultDto>>.SuccessResponse(studentResults, "Student results retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<StudentResultDto>>.ErrorResponse("Failed to retrieve student results", ex.Message));
            }
        }

        // Get all assessment visibilities for a section
        [HttpGet("sections/{sectionId}/assessments/visibility")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SectionAssessmentVisibilityDto>>>> GetAssessmentVisibilities(int sectionId)
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);

                if (section == null)
                {
                    return NotFound(ApiResponse<IEnumerable<SectionAssessmentVisibilityDto>>.ErrorResponse("Section not found."));
                }

                if (section.InstructorId != instructorId)
                {
                    return Forbid("You don't have access to this section.");
                }

                // Get all assessments (they're globally available)
                var allAssessments = await _unitOfWork.Assessments.GetAllAsync();
                
                // Get existing visibility settings for this section
                var existingVisibilities = await _unitOfWork.AssessmentVisibilities.GetBySectionIdAsync(sectionId);
                var visibilityDict = existingVisibilities.ToDictionary(av => av.AssessmentId, av => av.IsVisible);

                // Create assessment visibility DTOs for all assessments
                var assessmentVisibilities = allAssessments.Select(assessment =>
                {
                    // Default to visible if no explicit visibility record exists
                    var isVisible = visibilityDict.GetValueOrDefault(assessment.AssessmentID, true);
                    
                    return new SectionAssessmentVisibilityDto
                    {
                        SectionId = sectionId,
                        AssessmentId = assessment.AssessmentID,
                        IsVisible = isVisible,
                        Assessment = new AssessmentDto
                        {
                            AssessmentId = assessment.AssessmentID,
                            Title = assessment.Title,
                            Description = assessment.Description,
                            MaxScore = assessment.MaxScore,
                            DueDate = assessment.DueDate
                        }
                    };
                }).ToList();

                return Ok(ApiResponse<IEnumerable<SectionAssessmentVisibilityDto>>.SuccessResponse(assessmentVisibilities, "Assessment visibilities retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<IEnumerable<SectionAssessmentVisibilityDto>>.ErrorResponse("Unauthorized access", ex.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssessmentVisibilities: {ex.Message}");
                return StatusCode(500, ApiResponse<IEnumerable<SectionAssessmentVisibilityDto>>.ErrorResponse("Failed to retrieve assessment visibilities", ex.Message));
            }
        }

        // Update assessment visibility for a section
        [HttpPut("sections/{sectionId}/assessments/{assessmentId}/visibility")]
        public async Task<ActionResult<ApiResponse<SectionAssessmentVisibilityDto>>> UpdateAssessmentVisibility(int sectionId, int assessmentId, [FromBody] bool isVisible)
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var section = await _unitOfWork.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId && s.InstructorId == instructorId);

                if (section == null)
                {
                    return NotFound(ApiResponse<SectionAssessmentVisibilityDto>.ErrorResponse("Section not found or you don't have access to it."));
                }

                // Verify the assessment exists
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
                if (assessment == null)
                {
                    return NotFound(ApiResponse<SectionAssessmentVisibilityDto>.ErrorResponse("Assessment not found."));
                }

                var visibility = await _unitOfWork.AssessmentVisibilities.GetBySectionAndAssessmentAsync(sectionId, assessmentId);

                if (visibility == null)
                {
                    // Only create a record if setting to non-default (false)
                    // Default is true (visible), so only store when hiding
                    if (!isVisible)
                    {
                        visibility = new SectionAssessmentVisibility
                        {
                            SectionId = sectionId,
                            AssessmentId = assessmentId,
                            IsVisible = false
                        };
                        await _unitOfWork.AssessmentVisibilities.AddAsync(visibility);
                    }
                    // If setting to visible (default), no need to store a record
                }
                else
                {
                    // Update existing record
                    visibility.IsVisible = isVisible;
                    await _unitOfWork.AssessmentVisibilities.UpdateAsync(visibility);
                    
                    // If setting back to default (visible), we can optionally delete the record
                    // to keep the database clean, but keeping it for audit trail
                }

                await _unitOfWork.SaveChangesAsync();
                
                // Return the effective visibility status
                var result = new SectionAssessmentVisibilityDto
                {
                    SectionId = sectionId,
                    AssessmentId = assessmentId,
                    IsVisible = isVisible,
                    Assessment = new AssessmentDto
                    {
                        AssessmentId = assessment.AssessmentID,
                        Title = assessment.Title,
                        Description = assessment.Description,
                        MaxScore = assessment.MaxScore,
                        DueDate = assessment.DueDate
                    }
                };
                
                return Ok(ApiResponse<SectionAssessmentVisibilityDto>.SuccessResponse(result, "Assessment visibility updated successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating assessment visibility: {ex.Message}");
                return StatusCode(500, ApiResponse<SectionAssessmentVisibilityDto>.ErrorResponse("Failed to update assessment visibility", ex.Message));
            }
        }

        // Bulk update assessment visibility for a section
        [HttpPut("sections/{sectionId}/assessments/visibility/bulk")]
        public async Task<ActionResult<ApiResponse<string>>> BulkUpdateAssessmentVisibility(int sectionId, [FromBody] Dictionary<int, bool> assessmentVisibilities)
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var section = await _unitOfWork.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId && s.InstructorId == instructorId);

                if (section == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse("Section not found or you don't have access to it."));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.AssessmentVisibilities.BulkUpdateVisibilityAsync(sectionId, assessmentVisibilities);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    
                    return Ok(ApiResponse<string>.SuccessResponse("Success", "Assessment visibilities updated successfully"));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to update assessment visibilities", ex.Message));
            }
        }

        private int GetCurrentInstructorId()
        {
            var instructorIdClaim = User.FindFirst("InstructorId")?.Value;
            if (string.IsNullOrEmpty(instructorIdClaim))
            {
                throw new UnauthorizedAccessException("Instructor ID not found in token claims");
            }
            return int.Parse(instructorIdClaim);
        }
    }
}
