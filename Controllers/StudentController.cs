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

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
      private readonly PolarisDbContext _context;
      private readonly UserManager<ApplicationUser> _userManager;

      public StudentController(PolarisDbContext context, UserManager<ApplicationUser> userManager)
      {
          _context = context;
          _userManager = userManager;
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

}
