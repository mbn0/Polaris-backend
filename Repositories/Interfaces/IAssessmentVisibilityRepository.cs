using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IAssessmentVisibilityRepository : IBaseRepository<SectionAssessmentVisibility>
    {
        Task<SectionAssessmentVisibility?> GetBySectionAndAssessmentAsync(int sectionId, int assessmentId);
        Task<IEnumerable<SectionAssessmentVisibility>> GetBySectionIdAsync(int sectionId);
        Task BulkUpdateVisibilityAsync(int sectionId, Dictionary<int, bool> assessmentVisibilities);
    }
} 