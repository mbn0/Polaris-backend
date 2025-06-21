using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class InstructorRepository : BaseRepository<Instructor>, IInstructorRepository
    {
        public InstructorRepository(PolarisDbContext context) : base(context)
        {
        }

        public async Task<Instructor?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.UserId == userId);
        }

        public async Task<Instructor?> GetInstructorWithUserAsync(int instructorId)
        {
            return await _dbSet
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InstructorId == instructorId);
        }

        public async Task<IEnumerable<Instructor>> GetInstructorsWithUsersAsync()
        {
            return await _dbSet
                .Include(i => i.User)
                .ToListAsync();
        }
    }
} 