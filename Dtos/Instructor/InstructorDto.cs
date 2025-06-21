namespace backend.Dtos.Instructor
{
    public class InstructorDto
    {
        public int InstructorId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? UserId { get; set; }
        public int SectionsCount { get; set; }
    }
}