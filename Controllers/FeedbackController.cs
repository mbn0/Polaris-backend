using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.Dtos.Feedback;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // POST: api/feedback
        // Create feedback - accessible by Students and Instructors
        [HttpPost]
        [Authorize(Roles = "Student,Instructor")]
        public async Task<ActionResult<FeedbackDto>> CreateFeedback([FromBody] CreateFeedbackDto createFeedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found.");
            }

            var feedback = new Feedback
            {
                Subject = createFeedbackDto.Subject,
                Message = createFeedbackDto.Message,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            };

            await _unitOfWork.Feedbacks.AddAsync(feedback);
            await _unitOfWork.SaveChangesAsync();

            // Get the created feedback with user details
            var createdFeedback = await _unitOfWork.Feedbacks.GetByIdWithUserAsync(feedback.FeedbackId);
            if (createdFeedback == null)
            {
                return BadRequest("Failed to create feedback.");
            }

            var userRoles = await _userManager.GetRolesAsync(createdFeedback.User!);
            var userRole = userRoles.FirstOrDefault() ?? "Unknown";

            var feedbackDto = new FeedbackDto
            {
                FeedbackId = createdFeedback.FeedbackId,
                Subject = createdFeedback.Subject,
                Message = createdFeedback.Message,
                CreatedAt = createdFeedback.CreatedAt,
                IsResolved = createdFeedback.IsResolved,
                UserFullName = createdFeedback.User?.FullName ?? "",
                UserEmail = createdFeedback.User?.Email ?? "",
                UserRole = userRole
            };

            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.FeedbackId }, feedbackDto);
        }

        // GET: api/feedback/{id}
        // Get single feedback - accessible by Admins and the user who created it
        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackDto>> GetFeedback(int id)
        {
            var feedback = await _unitOfWork.Feedbacks.GetByIdWithUserAsync(id);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Check if user can access this feedback (admin or owner)
            if (!isAdmin && feedback.UserId != currentUserId)
            {
                return Forbid("You can only view your own feedback.");
            }

            var userRoles = await _userManager.GetRolesAsync(feedback.User!);
            var userRole = userRoles.FirstOrDefault() ?? "Unknown";

            var feedbackDto = new FeedbackDto
            {
                FeedbackId = feedback.FeedbackId,
                Subject = feedback.Subject,
                Message = feedback.Message,
                CreatedAt = feedback.CreatedAt,
                IsResolved = feedback.IsResolved,
                UserFullName = feedback.User?.FullName ?? "",
                UserEmail = feedback.User?.Email ?? "",
                UserRole = userRole
            };

            return Ok(feedbackDto);
        }

        // GET: api/feedback
        // Get all feedback - accessible by Admins only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<FeedbackListDto>>> GetAllFeedback()
        {
            var feedbacks = await _unitOfWork.Feedbacks.GetAllWithUsersAsync();

            var feedbackDtos = new List<FeedbackListDto>();
            foreach (var feedback in feedbacks)
            {
                var userRoles = await _userManager.GetRolesAsync(feedback.User!);
                var userRole = userRoles.FirstOrDefault() ?? "Unknown";

                feedbackDtos.Add(new FeedbackListDto
                {
                    FeedbackId = feedback.FeedbackId,
                    Subject = feedback.Subject,
                    CreatedAt = feedback.CreatedAt,
                    IsResolved = feedback.IsResolved,
                    UserFullName = feedback.User?.FullName ?? "",
                    UserRole = userRole
                });
            }

            return Ok(feedbackDtos);
        }

        // GET: api/feedback/my
        // Get current user's feedback - accessible by Students and Instructors
        [HttpGet("my")]
        [Authorize(Roles = "Student,Instructor")]
        public async Task<ActionResult<IEnumerable<FeedbackListDto>>> GetMyFeedback()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found.");
            }

            var feedbacks = await _unitOfWork.Feedbacks.GetByUserIdAsync(userId);

            var feedbackDtos = feedbacks.Select(feedback => new FeedbackListDto
            {
                FeedbackId = feedback.FeedbackId,
                Subject = feedback.Subject,
                CreatedAt = feedback.CreatedAt,
                IsResolved = feedback.IsResolved,
                UserFullName = feedback.User?.FullName ?? "",
                UserRole = "You"
            }).ToList();

            return Ok(feedbackDtos);
        }

        // GET: api/feedback/unresolved
        // Get unresolved feedback - accessible by Admins only
        [HttpGet("unresolved")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<FeedbackListDto>>> GetUnresolvedFeedback()
        {
            var feedbacks = await _unitOfWork.Feedbacks.GetUnresolvedAsync();

            var feedbackDtos = new List<FeedbackListDto>();
            foreach (var feedback in feedbacks)
            {
                var userRoles = await _userManager.GetRolesAsync(feedback.User!);
                var userRole = userRoles.FirstOrDefault() ?? "Unknown";

                feedbackDtos.Add(new FeedbackListDto
                {
                    FeedbackId = feedback.FeedbackId,
                    Subject = feedback.Subject,
                    CreatedAt = feedback.CreatedAt,
                    IsResolved = feedback.IsResolved,
                    UserFullName = feedback.User?.FullName ?? "",
                    UserRole = userRole
                });
            }

            return Ok(feedbackDtos);
        }

        // DELETE: api/feedback/{id}
        // Delete feedback - accessible by Admins only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _unitOfWork.Feedbacks.GetByIdAsync(id);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }

            await _unitOfWork.Feedbacks.DeleteAsync(feedback);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/feedback/{id}/resolve
        // Mark feedback as resolved - accessible by Admins only
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveFeedback(int id)
        {
            var feedback = await _unitOfWork.Feedbacks.GetByIdAsync(id);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }

            feedback.IsResolved = true;
            await _unitOfWork.Feedbacks.UpdateAsync(feedback);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Feedback marked as resolved." });
        }

        // PUT: api/feedback/{id}/unresolve
        // Mark feedback as unresolved - accessible by Admins only
        [HttpPut("{id}/unresolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnresolveFeedback(int id)
        {
            var feedback = await _unitOfWork.Feedbacks.GetByIdAsync(id);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }

            feedback.IsResolved = false;
            await _unitOfWork.Feedbacks.UpdateAsync(feedback);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Feedback marked as unresolved." });
        }
    }
} 