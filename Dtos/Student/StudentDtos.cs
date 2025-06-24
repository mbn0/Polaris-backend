using backend.Dtos.Instructor;
using backend.Dtos.Assessment;

namespace backend.Dtos.Student
{
    public class StudentSectionDto
    {
        public int SectionId { get; set; }
        public StudentInstructorDto? Instructor { get; set; }
        public ICollection<StudentAssessmentVisibilityDto> AssessmentVisibilities { get; set; } = new List<StudentAssessmentVisibilityDto>();
    }

    public class StudentInstructorDto
    {
        public int InstructorId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public StudentUserDto User { get; set; } = new StudentUserDto();
    }

    public class StudentUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class StudentAssessmentVisibilityDto
    {
        public int AssessmentVisibilityId { get; set; }
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public bool IsVisible { get; set; }
        public StudentAssessmentDto Assessment { get; set; } = new StudentAssessmentDto();
    }

    public class StudentAssessmentDto
    {
        public int AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7); // Default to 1 week from now
        public int MaxScore { get; set; } = 100; // Default max score
    }

    public class StudentProfileDto
    {
        public int StudentId { get; set; }
        public string MatricNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty; 
    }
} 