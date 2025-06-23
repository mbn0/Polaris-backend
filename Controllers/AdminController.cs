using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos.Instructor;
using backend.Dtos.Common;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Repositories.Interfaces;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AdminController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        // ----------------------------------------------------------------
        // SECTION MANAGEMENT
        // ----------------------------------------------------------------

        [HttpGet("sections")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSections()
        {
            var sections = await _unitOfWork.Sections.GetSectionsWithInstructorsAndStudentsAsync();

            var sectionDtos = sections.Select(s => new SectionDto
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

            return sectionDtos;
        }

        [HttpGet("sections/{id}")]
        public async Task<ActionResult<SectionDto>> GetSection(int id)
        {
            var sections = await _unitOfWork.Sections.GetSectionsWithInstructorsAndStudentsAsync();
            var s = sections.FirstOrDefault(x => x.SectionId == id);

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
            var instructor = await _unitOfWork.Instructors.GetByUserIdAsync(dto.InstructorUserId);
            if (instructor == null)
                return BadRequest("Instructor not found");

            var sec = new Section { InstructorId = instructor.InstructorId };
            await _unitOfWork.Sections.AddAsync(sec);
            await _unitOfWork.SaveChangesAsync();

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
            var sec = await _unitOfWork.Sections.GetByIdAsync(id);
            if (sec == null) return NotFound();

            var instructor = await _unitOfWork.Instructors.GetByUserIdAsync(dto.InstructorUserId);
            if (instructor == null)
                return BadRequest("Instructor not found");

            sec.InstructorId = instructor.InstructorId;
            await _unitOfWork.Sections.UpdateAsync(sec);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("sections/{id}")]
        public async Task<ActionResult> DeleteSection(int id)
        {
            var sec = await _unitOfWork.Sections.GetByIdAsync(id);
            if (sec == null) return NotFound();
            
            await _unitOfWork.Sections.DeleteAsync(sec);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("sections/{sectionId}/students/{userId}")]
        public async Task<ActionResult> AddStudentToSection(int sectionId, string userId)
        {
            var student = await _unitOfWork.Students.GetByUserIdAsync(userId);
            if (student == null) return NotFound("Student not found");
            
            student.SectionId = sectionId;
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("sections/{sectionId}/students/{userId}")]
        public async Task<ActionResult> RemoveStudentFromSection(int sectionId, string userId)
        {
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(st => st.UserId == userId && st.SectionId == sectionId);
            if (student == null) return NotFound();
            
            student.SectionId = null;
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        // ----------------------------------------------------------------
        // USER MANAGEMENT
        // ----------------------------------------------------------------

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FullName = u.FullName,
                    Roles = roles
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
                Id = u.Id,
                Email = u.Email!,
                FullName = u.FullName,
                Roles = roles
            };
        }

        [HttpGet("instructors")]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructors()
        {
            var instructors = await _unitOfWork.Instructors.GetInstructorsWithUsersAsync();

            return instructors.Select(i => new InstructorDto
            {
                InstructorId = i.InstructorId,
                FullName = i.User?.FullName ?? "",
                // Add any additional properties needed from the Admin version
            }).ToList();
        }

        [HttpPost("users")]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var u = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName
                };
                var createRes = await _userManager.CreateAsync(u, dto.Password);
                if (!createRes.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(createRes.Errors);
                }

                // assign roles
                foreach (var role in dto.Roles)
                    await _userManager.AddToRoleAsync(u, role);

                // if student, create profile
                if (dto.Roles.Contains("Student"))
                {
                    await _unitOfWork.Students.AddAsync(new Student
                    {
                        UserId = u.Id,
                        MatricNo = dto.MatricNo!,
                        SectionId = dto.SectionId
                    });
                }
                // if instructor, create profile
                if (dto.Roles.Contains("Instructor"))
                {
                    await _unitOfWork.Instructors.AddAsync(new Instructor
                    {
                        UserId = u.Id
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return CreatedAtAction(nameof(GetUser), new { id = u.Id }, null);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            // Update basic user properties
            u.Email = dto.Email;
            u.UserName = dto.Email;
            u.FullName = dto.FullName;
            
            // Only update password if it's provided
            if (!string.IsNullOrEmpty(dto.Password))
            {
                // Remove existing password and add new one
                var removePasswordResult = await _userManager.RemovePasswordAsync(u);
                if (removePasswordResult.Succeeded)
                {
                    var addPasswordResult = await _userManager.AddPasswordAsync(u, dto.Password);
                    if (!addPasswordResult.Succeeded)
                    {
                        return BadRequest(new { message = "Failed to update password", errors = addPasswordResult.Errors });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Failed to remove existing password", errors = removePasswordResult.Errors });
                }
            }
            // If password is null or empty, we keep the existing password unchanged

            // Update user roles
            var currentRoles = await _userManager.GetRolesAsync(u);
            // remove outdated roles
            await _userManager.RemoveFromRolesAsync(u, currentRoles.Except(dto.Roles));
            // add new roles
            await _userManager.AddToRolesAsync(u, dto.Roles.Except(currentRoles));

            // Update the user in Identity
            var updateResult = await _userManager.UpdateAsync(u);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new { message = "Failed to update user", errors = updateResult.Errors });
            }

            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("users/{id}/password")]
        public async Task<ActionResult> UpdateUserPassword(string id, [FromBody] UpdatePasswordDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            if (string.IsNullOrEmpty(dto.NewPassword))
            {
                return BadRequest(new { message = "New password is required" });
            }

            // Remove existing password and add new one
            var removePasswordResult = await _userManager.RemovePasswordAsync(u);
            if (removePasswordResult.Succeeded)
            {
                var addPasswordResult = await _userManager.AddPasswordAsync(u, dto.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    return BadRequest(new { message = "Failed to update password", errors = addPasswordResult.Errors });
                }
            }
            else
            {
                return BadRequest(new { message = "Failed to remove existing password", errors = removePasswordResult.Errors });
            }

            return Ok(new { message = "Password updated successfully" });
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
}

