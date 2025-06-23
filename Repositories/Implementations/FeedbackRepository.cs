using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class FeedbackRepository : BaseRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(PolarisDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Feedback>> GetAllWithUsersAsync()
        {
            return await _dbSet
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdWithUserAsync(int feedbackId)
        {
            return await _dbSet
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
        }

        public async Task<IEnumerable<Feedback>> GetUnresolvedAsync()
        {
            return await _dbSet
                .Where(f => !f.IsResolved)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
} 