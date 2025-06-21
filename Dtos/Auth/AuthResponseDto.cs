namespace backend.Dtos.Auth
{
    public class AuthResponseDto
    {
        public string FullName   { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
        public string? MatricNo  { get; set; }
        public int?   SectionId  { get; set; }
        // you could add more instructor- or admin-specific props here
        public string Token      { get; set; } = string.Empty;
    }
}
