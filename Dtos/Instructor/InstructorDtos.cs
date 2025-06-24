using backend.Dtos.Common;

namespace backend.Dtos.Instructor
{
    public class InstructorSectionDto
    {
        public int SectionId { get; set; }
        public int InstructorId { get; set; }
        public List<StudentBriefDto> Students { get; set; } = new List<StudentBriefDto>();
        public List<AssessmentVisibilityDto> AssessmentVisibilities { get; set; } = new List<AssessmentVisibilityDto>();
    }

    public class AssessmentVisibilityDto
    {
        public int AssessmentId { get; set; }
        public string AssessmentTitle { get; set; } = string.Empty;
        public string AssessmentDescription { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
    }

    public class StudentResultDto
    {
        public int StudentId { get; set; }
        public string MatricNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<ResultDto> Results { get; set; } = new List<ResultDto>();
    }

    public class ResultDto
    {
        public int AssessmentId { get; set; }
        public string AssessmentTitle { get; set; } = string.Empty;
        public float Score { get; set; }
        public DateTime DateTaken { get; set; }
    }
} 