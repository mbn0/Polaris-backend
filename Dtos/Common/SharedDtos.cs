namespace backend.Dtos.Common
{
    public class StudentBriefDto
    {
        public int StudentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MatricNo { get; set; } = string.Empty;
    }

    public class SectionDto
    {
        public int SectionId { get; set; }
        public string InstructorUserId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public List<StudentBriefDto> Students { get; set; } = new();
    }

    public class CreateSectionResponseDto
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public string InstructorUserId { get; set; } = string.Empty;
    }

    public class CreateSectionDto
    {
        public string InstructorUserId { get; set; } = string.Empty;
    }

    public class UpdateSectionDto
    {
        public string InstructorUserId { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id       { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class CreateUserDto
    {
        public string Email     { get; set; } = string.Empty;
        public string FullName  { get; set; } = string.Empty;
        public string Password  { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string? MatricNo   { get; set; }
        public int? SectionId     { get; set; }
    }

    public class UpdateUserDto
    {
        public string Email     { get; set; } = string.Empty;
        public string FullName  { get; set; } = string.Empty;
        /// <summary>
        /// Optional: Only provide if you want to change the user's password
        /// Leave null or empty to keep existing password unchanged
        /// </summary>
        public string? Password { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UpdatePasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
} 