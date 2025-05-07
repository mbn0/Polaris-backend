
using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class ApplicationUser : IdentityUser
    {
      public string FullName { get; set; } = string.Empty;

      // Navigation properties
      public Student? StudentProfile { get; set; }
      public Instructor? InstructorProfile { get; set; }
    }
}
