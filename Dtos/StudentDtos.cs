namespace backend.Dtos
{
    public class StudentSectionDto
    {
        public int SectionId { get; set; }
        public InstructorDto? Instructor { get; set; }
        public ICollection<AssessmentDto> Assessments { get; set; } = new List<AssessmentDto>();
    }
} 