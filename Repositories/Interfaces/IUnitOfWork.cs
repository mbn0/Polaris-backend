namespace backend.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ISectionRepository Sections { get; }
        IStudentRepository Students { get; }
        IInstructorRepository Instructors { get; }
        IAssessmentVisibilityRepository AssessmentVisibilities { get; }
        IResultRepository Results { get; }
        IAssessmentRepository Assessments { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
} 