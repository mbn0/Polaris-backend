using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class SectionRepository : BaseRepository<Section>, ISectionRepository
    {
        public SectionRepository(PolarisDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Section>> GetSectionsByInstructorIdAsync(int instructorId)
        {
            return await _dbSet
                .Where(s => s.InstructorId == instructorId)
                .Include(s => s.Students!)
                    .ThenInclude(student => student.User)
                .Include(s => s.AssessmentVisibilities)
                    .ThenInclude(av => av.Assessment)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Section?> GetSectionWithStudentsAsync(int sectionId)
        {
            return await _dbSet
                .Include(s => s.Students!)
                    .ThenInclude(student => student.User)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);
        }

        public async Task<Section?> GetSectionWithStudentsAndResultsAsync(int sectionId)
        {
            return await _dbSet
                .Include(s => s.Students!)
                    .ThenInclude(student => student.User)
                .Include(s => s.Students!)
                    .ThenInclude(student => student.Results!)
                        .ThenInclude(result => result.Assessment)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);
        }

        public async Task<Section?> GetSectionWithAssessmentVisibilitiesAsync(int sectionId)
        {
            return await _dbSet
                .Include(s => s.AssessmentVisibilities)
                    .ThenInclude(av => av.Assessment)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);
        }

        public async Task<Section?> GetSectionForInstructorAsync(int sectionId, int instructorId)
        {
            return await _dbSet
                .Where(s => s.SectionId == sectionId && s.InstructorId == instructorId)
                .Include(s => s.Students!)
                    .ThenInclude(student => student.User)
                .Include(s => s.Students!)
                    .ThenInclude(student => student.Results)
                .Include(s => s.AssessmentVisibilities)
                    .ThenInclude(av => av.Assessment)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Section>> GetSectionsWithInstructorsAndStudentsAsync()
        {
            return await _dbSet
                .Include(s => s.Instructor)
                    .ThenInclude(i => i!.User)
                .Include(s => s.Students!)
                    .ThenInclude(st => st.User)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<IEnumerable<Section>> GetSectionsWithDetailsAsync()
        {
            return await _dbSet
                .Include(s => s.Instructor)
                    .ThenInclude(i => i!.User)
                .Include(s => s.Students!)
                    .ThenInclude(st => st.User)
                .Include(s => s.AssessmentVisibilities)
                    .ThenInclude(av => av.Assessment)
                .AsSplitQuery()
                .ToListAsync();
        }
    }
} 