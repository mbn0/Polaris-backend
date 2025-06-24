using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Data
{
  public class PolarisDbContext : IdentityDbContext<ApplicationUser>
  {
    public PolarisDbContext(DbContextOptions<PolarisDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; } = default!;
    public DbSet<Instructor> Instructors { get; set; } = default!;
    public DbSet<Section> Sections { get; set; } = default!;
    public DbSet<Assessment> Assessments { get; set; } = default!;
    public DbSet<Result> Results { get; set; } = default!;
    public DbSet<SectionAssessmentVisibility> SectionAssessmentVisibilities { get; set; } = default!;
    public DbSet<Feedback> Feedbacks { get; set; } = default!;

      protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        List<IdentityRole> roles = new List<IdentityRole>
        {
            new IdentityRole { Name = "Student", NormalizedName = "STUDENT" },
            new IdentityRole { Name = "Instructor", NormalizedName = "INSTRUCTOR" },
              new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" }
        };

        modelBuilder.Entity<IdentityRole>().HasData(roles);


        modelBuilder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.StudentProfile)
            .HasForeignKey<Student>(s => s.UserId);

        modelBuilder.Entity<Instructor>()
            .HasOne(i => i.User)
            .WithOne(u => u.InstructorProfile)
            .HasForeignKey<Instructor>(i => i.UserId);

        modelBuilder.Entity<Student>()
            .HasOne(s => s.Section)
            .WithMany(sec => sec.Students)
            .HasForeignKey(s => s.SectionId);

        modelBuilder.Entity<Section>()
            .HasOne(sec => sec.Instructor)
            .WithMany(ins => ins.Sections)
            .HasForeignKey(sec => sec.InstructorId);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Student)
            .WithMany(s => s.Results)
            .HasForeignKey(r => r.StudentId);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Assessment)
            .WithMany(a => a.Results)
            .HasForeignKey(r => r.AssessmentId)
            .HasPrincipalKey(a => a.AssessmentID);

        modelBuilder.Entity<SectionAssessmentVisibility>()
            .HasKey(sav => new { sav.SectionId, sav.AssessmentId });

        modelBuilder.Entity<SectionAssessmentVisibility>()
            .HasOne(sav => sav.Section)
            .WithMany(s => s.AssessmentVisibilities)
            .HasForeignKey(sav => sav.SectionId);

        modelBuilder.Entity<SectionAssessmentVisibility>()
            .HasOne(sav => sav.Assessment)
            .WithMany(a => a.SectionVisibilities)
            .HasForeignKey(sav => sav.AssessmentId);

        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
}
