using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IInstructorRepository : IBaseRepository<Instructor>
    {
        Task<Instructor?> GetByUserIdAsync(string userId);
        Task<Instructor?> GetInstructorWithUserAsync(int instructorId);
        Task<IEnumerable<Instructor>> GetInstructorsWithUsersAsync();
    }
} 