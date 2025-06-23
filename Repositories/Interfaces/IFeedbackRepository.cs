using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IFeedbackRepository : IBaseRepository<Feedback>
    {
        Task<IEnumerable<Feedback>> GetAllWithUsersAsync();
        Task<IEnumerable<Feedback>> GetByUserIdAsync(string userId);
        Task<Feedback?> GetByIdWithUserAsync(int feedbackId);
        Task<IEnumerable<Feedback>> GetUnresolvedAsync();
    }
} 