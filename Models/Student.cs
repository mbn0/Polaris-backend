 
 
namespace backend.Models
{

    public class Student 
    {
        public int StudentId { get; set; }
        public string MatricNo { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int? SectionId { get; set; }
        public Section? Section { get; set; }


        public ICollection<Result>? Results { get; set; } 

    }
}
