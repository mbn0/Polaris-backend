using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IAssessmentRepository : IBaseRepository<Assessment>
    {
        Task<IEnumerable<Assessment>> GetVisibleAssessmentsBySectionIdAsync(int sectionId);
        Task<Assessment?> GetAssessmentWithResultsAsync(int assessmentId);
    }
} 