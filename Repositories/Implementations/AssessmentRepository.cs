using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class AssessmentRepository : BaseRepository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(PolarisDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Assessment>> GetVisibleAssessmentsBySectionIdAsync(int sectionId)
        {
            return await _context.SectionAssessmentVisibilities
                .Where(sav => sav.SectionId == sectionId && sav.IsVisible)
                .Select(sav => sav.Assessment)
                .ToListAsync();
        }

        public async Task<Assessment?> GetAssessmentWithResultsAsync(int assessmentId)
        {
            return await _dbSet
                .Include(a => a.Results!)
                    .ThenInclude(r => r.Student)
                        .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);
        }
    }
} 