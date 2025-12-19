using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;

namespace StudentCrm.Persistence.Context
{

    public class StudentCrmDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public StudentCrmDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AppUser> Admins { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<TeacherUser> Teachers { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<StudentUser> UniStudents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Applies all IEntityTypeConfiguration<> classes in this assembly
            builder.ApplyConfigurationsFromAssembly(typeof(StudentCrmDbContext).Assembly);
          
        }

    }
}

