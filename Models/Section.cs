namespace backend.Models
{

  public class Section
  {
    public int SectionId { get; set; }

    public int InstructorId { get; set; }
    public Instructor? Instructor { get; set; }

    public ICollection<Student>? Students { get; set; }
    public ICollection<Result>? Results { get; set; }

    public ICollection<SectionAssessmentVisibility> AssessmentVisibilities { get; set; } = new List<SectionAssessmentVisibility>();
  }
}
