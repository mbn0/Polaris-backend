namespace backend.Models
{

  public class Assessment
  {
    public int AssessmentID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Result> Results { get; set; } = new List<Result>();
  }
}
