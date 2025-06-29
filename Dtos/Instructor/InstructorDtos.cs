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
        public List<InstructorResultDto> Results { get; set; } = new List<InstructorResultDto>();
    }

    public class InstructorResultDto
    {
        public int AssessmentId { get; set; }
        public string AssessmentTitle { get; set; } = string.Empty;
        public float Score { get; set; }
        public DateTime DateTaken { get; set; }
    }

    public class SectionAssessmentVisibilityDto
    {
        public int SectionId { get; set; }
        public int AssessmentId { get; set; }
        public bool IsVisible { get; set; }
        public AssessmentDto Assessment { get; set; } = new AssessmentDto();
    }

    public class AssessmentDto
    {
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public DateTime DueDate { get; set; }
    }
} 