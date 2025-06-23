using Microsoft.EntityFrameworkCore.Storage;
using backend.Data;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PolarisDbContext _context;
        private IDbContextTransaction? _transaction;

        private ISectionRepository? _sections;
        private IStudentRepository? _students;
        private IInstructorRepository? _instructors;
        private IAssessmentVisibilityRepository? _assessmentVisibilities;
        private IResultRepository? _results;
        private IAssessmentRepository? _assessments;
        private IFeedbackRepository? _feedbacks;

        public UnitOfWork(PolarisDbContext context)
        {
            _context = context;
        }

        public ISectionRepository Sections =>
            _sections ??= new SectionRepository(_context);

        public IStudentRepository Students =>
            _students ??= new StudentRepository(_context);

        public IInstructorRepository Instructors =>
            _instructors ??= new InstructorRepository(_context);

        public IAssessmentVisibilityRepository AssessmentVisibilities =>
            _assessmentVisibilities ??= new AssessmentVisibilityRepository(_context);

        public IResultRepository Results =>
            _results ??= new ResultRepository(_context);

        public IAssessmentRepository Assessments =>
            _assessments ??= new AssessmentRepository(_context);

        public IFeedbackRepository Feedbacks =>
            _feedbacks ??= new FeedbackRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
} 