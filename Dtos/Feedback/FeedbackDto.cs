using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Feedback
{
    public class CreateFeedbackDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Subject must be between 1 and 100 characters.")]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000, ErrorMessage = "Message must be between 1 and 1000 characters.")]
        public string Message { get; set; } = string.Empty;
    }
    
    public class FeedbackDto
    {
        public int FeedbackId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
    
    public class FeedbackListDto
    {
        public int FeedbackId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
} 