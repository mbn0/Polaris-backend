using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Auth
{
  public class RegistrationDto
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string MatricNo { get; set; } = string.Empty;
  }
}
