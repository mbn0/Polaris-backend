namespace backend.Models
{

  public class Assessment
  {
    public int AssessmentID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxScore { get; set; } = 100;
    public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

    public ICollection<Result> Results { get; set; } = new List<Result>();
    public ICollection<SectionAssessmentVisibility> SectionVisibilities { get; set; } = new List<SectionAssessmentVisibility>();
  }
}
