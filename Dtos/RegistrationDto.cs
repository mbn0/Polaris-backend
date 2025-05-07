
using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
  public class RegistrationDto
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string MatricNo { get; set; } = string.Empty;
  }
}
