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
                            .Select(r => new ResultDto
                            {
                                AssessmentId = r.Assessment?.AssessmentID ?? 0,
                                AssessmentTitle = r.Assessment?.Title ?? "",
                                Score = (decimal)r.Score,  // Explicit cast from float to decimal
                                DateTaken = r.Date  // Changed from SubmissionDate to Date
                            })
                            .ToList() ?? new List<ResultDto>()
                    })
                    .ToList() ?? new List<StudentResultDto>();

                return Ok(ApiResponse<IEnumerable<StudentResultDto>>.SuccessResponse(studentResults, "Student results retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<StudentResultDto>>.ErrorResponse("Failed to retrieve student results", ex.Message));
            }
        }

        // Get all assessment visibilities for a section
        [HttpGet("sections/{sectionId}/assessments/visibility")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SectionAssessmentVisibility>>>> GetAssessmentVisibilities(int sectionId)
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var section = await _unitOfWork.Sections.GetSectionWithAssessmentVisibilitiesAsync(sectionId);

                if (section == null || section.InstructorId != instructorId)
                {
                    return NotFound(ApiResponse<IEnumerable<SectionAssessmentVisibility>>.ErrorResponse("Section not found or you don't have access to it."));
                }

                return Ok(ApiResponse<IEnumerable<SectionAssessmentVisibility>>.SuccessResponse(section.AssessmentVisibilities, "Assessment visibilities retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<SectionAssessmentVisibility>>.ErrorResponse("Failed to retrieve assessment visibilities", ex.Message));
            }
        }

        // Update assessment visibility for a section
        [HttpPut("sections/{sectionId}/assessments/{assessmentId}/visibility")]
        public async Task<ActionResult<ApiResponse<SectionAssessmentVisibility>>> UpdateAssessmentVisibility(int sectionId, int assessmentId, [FromBody] bool isVisible)
        {
            try
            {
                var instructorId = GetCurrentInstructorId();
                var section = await _unitOfWork.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId && s.InstructorId == instructorId);

                if (section == null)
                {
                    return NotFound(ApiResponse<SectionAssessmentVisibility>.ErrorResponse("Section not found or you don't have access to it."));
                }

                var visibility = await _unitOfWork.AssessmentVisibilities.GetBySectionAndAssessmentAsync(sectionId, assessmentId);

                if (visibility == null)
                {
                    visibility = new SectionAssessmentVisibility
                    {
                        SectionId = sectionId,
                        AssessmentId = assessmentId,
                        IsVisible = isVisible
                    };
                    await _unitOfWork.AssessmentVisibilities.AddAsync(visibility);
                }
                else
                {
                    visibility.IsVisible = isVisible;
                    await _unitOfWork.AssessmentVisibilities.UpdateAsync(visibility);
                }

                await _unitOfWork.SaveChangesAsync();
                return Ok(ApiResponse<SectionAssessmentVisibility>.SuccessResponse(visibility, "Assessment visibility updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SectionAssessmentVisibility>.ErrorResponse("Failed to update assessment visibility", ex.Message));
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
