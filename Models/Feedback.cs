using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsResolved { get; set; } = false;
        
        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        
        // Navigation Properties
        public ApplicationUser? User { get; set; }
    }
} 