using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class AssessmentVisibilityRepository : BaseRepository<SectionAssessmentVisibility>, IAssessmentVisibilityRepository
    {
        public AssessmentVisibilityRepository(PolarisDbContext context) : base(context)
        {
        }

        public async Task<SectionAssessmentVisibility?> GetBySectionAndAssessmentAsync(int sectionId, int assessmentId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(av => av.SectionId == sectionId && av.AssessmentId == assessmentId);
        }

        public async Task<IEnumerable<SectionAssessmentVisibility>> GetBySectionIdAsync(int sectionId)
        {
            return await _dbSet
                .Where(av => av.SectionId == sectionId)
                .Include(av => av.Assessment)
                .ToListAsync();
        }

        public async Task BulkUpdateVisibilityAsync(int sectionId, Dictionary<int, bool> assessmentVisibilities)
        {
            foreach (var (assessmentId, isVisible) in assessmentVisibilities)
            {
                var visibility = await GetBySectionAndAssessmentAsync(sectionId, assessmentId);

                if (visibility == null)
                {
                    visibility = new SectionAssessmentVisibility
                    {
                        SectionId = sectionId,
                        AssessmentId = assessmentId,
                        IsVisible = isVisible
                    };
                    await AddAsync(visibility);
                }
                else
                {
                    visibility.IsVisible = isVisible;
                    await UpdateAsync(visibility);
                }
            }
        }
    }
} 