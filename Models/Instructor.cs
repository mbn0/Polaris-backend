namespace backend.Models
{
    public class Instructor
    {
      public int InstructorId { get; set; }
      public string UserId { get; set; } = string.Empty;

      // Navigation properties
      public ApplicationUser? User { get; set; }
      public ICollection<Section>? Sections { get; set; }
    }
}
