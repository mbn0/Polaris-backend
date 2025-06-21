using backend.Dtos.Instructor;
using backend.Dtos.Assessment;

namespace backend.Dtos.Student
{
    public class StudentSectionDto
    {
        public int SectionId { get; set; }
        public InstructorDto? Instructor { get; set; }
        public ICollection<AssessmentDto> Assessments { get; set; } = new List<AssessmentDto>();
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