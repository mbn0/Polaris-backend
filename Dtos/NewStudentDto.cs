// the object returend after creating a new student
namespace backend.Dtos
{
    public class NewStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string MatricNo { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int SectionId { get; set; }
    }
}
