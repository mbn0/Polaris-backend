using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class ResultRepository : BaseRepository<Result>, IResultRepository
    {
        public ResultRepository(PolarisDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Result>> GetResultsByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .Where(r => r.StudentId == studentId)
                .Include(r => r.Assessment)
                .ToListAsync();
        }

        public async Task<IEnumerable<Result>> GetResultsByAssessmentIdAsync(int assessmentId)
        {
            return await _dbSet
                .Where(r => r.AssessmentId == assessmentId)
                .Include(r => r.Student)
                    .ThenInclude(s => s!.User)
                .ToListAsync();
        }

        public async Task<Result?> GetResultByStudentAndAssessmentAsync(int studentId, int assessmentId)
        {
            return await _dbSet
                .Include(r => r.Assessment)
                .Include(r => r.Student)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(r => r.StudentId == studentId && r.AssessmentId == assessmentId);
        }

        public async Task<IEnumerable<Result>> GetResultsWithAssessmentsAsync()
        {
            return await _dbSet
                .Include(r => r.Assessment)
                .Include(r => r.Student)
                    .ThenInclude(s => s!.User)
                .ToListAsync();
        }
    }
} 