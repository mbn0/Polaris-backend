namespace backend.Models
{
public class Result
{
    public int ResultId { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public float Score { get; set; }
    public DateTime Date { get; set; }
}
}
