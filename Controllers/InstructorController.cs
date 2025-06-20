using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using backend.Data;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Instructor, Admin")]
    public class InstructorController : ControllerBase
    {
        private readonly PolarisDbContext _context;

        public InstructorController(PolarisDbContext context)
        {
            _context = context;
        }

        // Get all sections assigned to the instructor
        [HttpGet("sections")]
        public async Task<ActionResult<IEnumerable<Section>>> GetInstructorSections()
        {
            var instructorId = GetCurrentInstructorId();
            var sections = await _context.Sections
                .Where(s => s.InstructorId == instructorId)
                .Include(s => s.Students)
                .Include(s => s.AssessmentVisibilities)
                    .ThenInclude(av => av.Assessment)
                .ToListAsync();

            return Ok(sections);
        }

        // Get all assessment visibilities for a section
        [HttpGet("sections/{sectionId}/assessments/visibility")]
        public async Task<ActionResult<IEnumerable<SectionAssessmentVisibility>>> GetAssessmentVisibilities(int sectionId)
        {
            var section = await _context.Sections
                .Include(s => s.AssessmentVisibilities)
                .ThenInclude(av => av.Assessment)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.InstructorId == GetCurrentInstructorId());

            if (section == null)
            {
                return NotFound("Section not found or you don't have access to it.");
            }

            return Ok(section.AssessmentVisibilities);
        }

        // Update assessment visibility for a section
        [HttpPut("sections/{sectionId}/assessments/{assessmentId}/visibility")]
        public async Task<IActionResult> UpdateAssessmentVisibility(int sectionId, int assessmentId, [FromBody] bool isVisible)
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.InstructorId == GetCurrentInstructorId());

            if (section == null)
            {
                return NotFound("Section not found or you don't have access to it.");
            }

            var visibility = await _context.SectionAssessmentVisibilities
                .FirstOrDefaultAsync(av => av.SectionId == sectionId && av.AssessmentId == assessmentId);

            if (visibility == null)
            {
                visibility = new SectionAssessmentVisibility
                {
                    SectionId = sectionId,
                    AssessmentId = assessmentId,
                    IsVisible = isVisible
                };
                _context.SectionAssessmentVisibilities.Add(visibility);
            }
            else
            {
                visibility.IsVisible = isVisible;
                _context.SectionAssessmentVisibilities.Update(visibility);
            }

            await _context.SaveChangesAsync();
            return Ok(visibility);
        }

        // Bulk update assessment visibility for a section
        [HttpPut("sections/{sectionId}/assessments/visibility/bulk")]
        public async Task<IActionResult> BulkUpdateAssessmentVisibility(int sectionId, [FromBody] Dictionary<int, bool> assessmentVisibilities)
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.InstructorId == GetCurrentInstructorId());

            if (section == null)
            {
                return NotFound("Section not found or you don't have access to it.");
            }

            foreach (var (assessmentId, isVisible) in assessmentVisibilities)
            {
                var visibility = await _context.SectionAssessmentVisibilities
                    .FirstOrDefaultAsync(av => av.SectionId == sectionId && av.AssessmentId == assessmentId);

                if (visibility == null)
                {
                    visibility = new SectionAssessmentVisibility
                    {
                        SectionId = sectionId,
                        AssessmentId = assessmentId,
                        IsVisible = isVisible
                    };
                    _context.SectionAssessmentVisibilities.Add(visibility);
                }
                else
                {
                    visibility.IsVisible = isVisible;
                    _context.SectionAssessmentVisibilities.Update(visibility);
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private int GetCurrentInstructorId()
        {
            // Assuming you have the instructor ID in the User Claims
            // You'll need to implement this based on your authentication setup
            return int.Parse(User.FindFirst("InstructorId")?.Value ?? "0");
        }
    }
}
