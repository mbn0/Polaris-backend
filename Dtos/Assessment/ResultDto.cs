namespace backend.Dtos.Assessment
{
    public class ResultDto
    {
        public int ResultId { get; set; }
        public int StudentId { get; set; }
        public int AssessmentId { get; set; }
        public string AssessmentTitle { get; set; } = string.Empty;
        public float Score { get; set; }
        public DateTime DateTaken { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string MatricNo { get; set; } = string.Empty;
    }

    public class SubmitResultDto
    {
        public int AssessmentId { get; set; }
        public float Score { get; set; }
        public DateTime DateTaken { get; set; }
    }

    public class UpdateResultDto
    {
        public int ResultId { get; set; }
        public float Score { get; set; }
        public DateTime DateTaken { get; set; }
    }

    public class StudentResultSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string MatricNo { get; set; } = string.Empty;
        public List<ResultDto> Results { get; set; } = new List<ResultDto>();
        public float AverageScore { get; set; }
        public int CompletedAssessments { get; set; }
    }
} 