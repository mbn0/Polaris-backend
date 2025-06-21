using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        Task<Student?> GetByUserIdAsync(string userId);
        Task<Student?> GetStudentWithSectionAsync(int studentId);
        Task<Student?> GetStudentWithUserAsync(int studentId);
        Task<IEnumerable<Student>> GetStudentsBySectionIdAsync(int sectionId);
        Task<Student?> GetByMatricNoAsync(string matricNo);
    }
} 