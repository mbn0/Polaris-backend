using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class SectionAssessmentVisibility
    {
        public int SectionId { get; set; }
        public Section Section { get; set; } = null!;

        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; } = null!;

        public bool IsVisible { get; set; }
    }
} 