using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class StudentRepository : BaseRepository<Student>, IStudentRepository
    {
        public StudentRepository(PolarisDbContext context) : base(context)
        {
        }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Student?> GetStudentWithSectionAsync(int studentId)
        {
            return await _dbSet
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<Student?> GetStudentWithUserAsync(int studentId)
        {
            return await _dbSet
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<IEnumerable<Student>> GetStudentsBySectionIdAsync(int sectionId)
        {
            return await _dbSet
                .Where(s => s.SectionId == sectionId)
                .Include(s => s.User)
                .ToListAsync();
        }

        public async Task<Student?> GetByMatricNoAsync(string matricNo)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.MatricNo == matricNo);
        }
    }
} 