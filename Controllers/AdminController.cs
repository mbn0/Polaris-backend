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
            try
            {
                // Verify instructor exists
                var instructor = await _unitOfWork.Instructors.GetByUserIdAsync(dto.InstructorUserId);
                if (instructor == null)
                    return BadRequest("Instructor not found");

                var sec = new Section { InstructorId = instructor.InstructorId };
                await _unitOfWork.Sections.AddAsync(sec);
                await _unitOfWork.SaveChangesAsync();

                // No need to create assessment visibilities - assessments are globally available
                // Visibility records are only created when instructors explicitly change visibility

                return new CreateSectionResponseDto
                {
                    Id = sec.SectionId,
                    InstructorId = instructor.InstructorId,
                    InstructorUserId = instructor.UserId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating section: {ex.Message}");
                return StatusCode(500, $"Internal server error while creating section: {ex.Message}");
            }
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
            
            // Remove students from this section (set their SectionId to null)
            var studentsInSection = await _unitOfWork.Students.FindAsync(s => s.SectionId == id);
            foreach (var student in studentsInSection)
            {
                student.SectionId = null;
                await _unitOfWork.Students.UpdateAsync(student);
            }
            
            // Delete related SectionAssessmentVisibility records
            var visibilityRecords = await _unitOfWork.AssessmentVisibilities.GetBySectionIdAsync(id);
            foreach (var visibility in visibilityRecords)
            {
                await _unitOfWork.AssessmentVisibilities.DeleteAsync(visibility);
            }
            
            // Now we can safely delete the section
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
                UserId = i.UserId,
                Email = i.User?.Email ?? ""
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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var u = await _userManager.FindByIdAsync(id);
                if (u == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound();
                }

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
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(new { message = "Failed to update password", errors = addPasswordResult.Errors });
                        }
                    }
                    else
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(new { message = "Failed to remove existing password", errors = removePasswordResult.Errors });
                    }
                }
                // If password is null or empty, we keep the existing password unchanged

                // Get current and new roles
                var currentRoles = await _userManager.GetRolesAsync(u);
                var rolesToRemove = currentRoles.Except(dto.Roles).ToList();
                var rolesToAdd = dto.Roles.Except(currentRoles).ToList();

                // Handle role-specific data before updating roles
                // If removing Student role, remove student record
                if (rolesToRemove.Contains("Student") && !dto.Roles.Contains("Student"))
                {
                    var student = await _unitOfWork.Students.GetByUserIdAsync(id);
                    if (student != null)
                    {
                        await _unitOfWork.Students.DeleteAsync(student);
                    }
                }

                // If removing Instructor role, remove instructor record
                if (rolesToRemove.Contains("Instructor") && !dto.Roles.Contains("Instructor"))
                {
                    var instructor = await _unitOfWork.Instructors.GetByUserIdAsync(id);
                    if (instructor != null)
                    {
                        await _unitOfWork.Instructors.DeleteAsync(instructor);
                    }
                }

                // If adding Student role, create student record
                if (rolesToAdd.Contains("Student") && !currentRoles.Contains("Student"))
                {
                    var existingStudent = await _unitOfWork.Students.GetByUserIdAsync(id);
                    if (existingStudent == null)
                    {
                        await _unitOfWork.Students.AddAsync(new Student
                        {
                            UserId = id,
                            MatricNo = dto.MatricNo ?? $"TEMP_{DateTime.Now.Ticks}", // Generate temp matric number if not provided
                            SectionId = dto.SectionId
                        });
                    }
                }

                // If adding Instructor role, create instructor record
                if (rolesToAdd.Contains("Instructor") && !currentRoles.Contains("Instructor"))
                {
                    var existingInstructor = await _unitOfWork.Instructors.GetByUserIdAsync(id);
                    if (existingInstructor == null)
                    {
                        await _unitOfWork.Instructors.AddAsync(new Instructor
                        {
                            UserId = id
                        });
                    }
                }

                // Update user roles
                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(u, rolesToRemove);
                }
                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(u, rolesToAdd);
                }

                // Update the user in Identity
                var updateResult = await _userManager.UpdateAsync(u);
                if (!updateResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { message = "Failed to update user", errors = updateResult.Errors });
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return NoContent();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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

        [HttpPost("test-data")]
        public async Task<ActionResult> CreateTestData()
        {
            try
            {
                // Create test assessments if none exist
                var existingAssessments = await _unitOfWork.Assessments.GetAllAsync();
                if (!existingAssessments.Any())
                {
                    var assessments = new List<Assessment>
                    {
                        new Assessment
                        {
                            Title = "Introduction to Cryptography Quiz",
                            Description = "Basic concepts of cryptography including symmetric and asymmetric encryption, hashing, and digital signatures."
                        },
                        new Assessment
                        {
                            Title = "AES Encryption Assignment",
                            Description = "Practical exercise on Advanced Encryption Standard implementation and analysis."
                        },
                        new Assessment
                        {
                            Title = "RSA Key Generation Lab",
                            Description = "Hands-on lab for understanding RSA key pair generation and digital signatures."
                        },
                        new Assessment
                        {
                            Title = "Hash Functions and MAC",
                            Description = "Assessment covering cryptographic hash functions and message authentication codes."
                        },
                        new Assessment
                        {
                            Title = "PKI and Digital Certificates",
                            Description = "Comprehensive exam on Public Key Infrastructure and certificate management."
                        }
                    };

                    await _unitOfWork.Assessments.AddRangeAsync(assessments);
                    await _unitOfWork.SaveChangesAsync();
                }

                return Ok(new { message = "Test assessments created successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating test data: {ex.Message}");
                return StatusCode(500, "Internal server error while creating test data");
            }
        }
    }
}

