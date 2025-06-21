using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface ISectionRepository : IBaseRepository<Section>
    {
        Task<IEnumerable<Section>> GetSectionsByInstructorIdAsync(int instructorId);
        Task<Section?> GetSectionWithStudentsAsync(int sectionId);
        Task<Section?> GetSectionWithStudentsAndResultsAsync(int sectionId);
        Task<Section?> GetSectionWithAssessmentVisibilitiesAsync(int sectionId);
        Task<Section?> GetSectionForInstructorAsync(int sectionId, int instructorId);
        Task<IEnumerable<Section>> GetSectionsWithInstructorsAndStudentsAsync();
    }
} 