using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IResultRepository : IBaseRepository<Result>
    {
        Task<IEnumerable<Result>> GetResultsByStudentIdAsync(int studentId);
        Task<IEnumerable<Result>> GetResultsByAssessmentIdAsync(int assessmentId);
        Task<Result?> GetResultByStudentAndAssessmentAsync(int studentId, int assessmentId);
        Task<IEnumerable<Result>> GetResultsWithAssessmentsAsync();
    }
} 