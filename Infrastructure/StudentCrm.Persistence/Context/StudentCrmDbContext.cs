using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;

namespace StudentCrm.Persistence.Context
{
    public class StudentCrmDbContext : IdentityDbContext<Admin>
    {
        public StudentCrmDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<UniStudent> UniStudents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Applies all IEntityTypeConfiguration<> classes in this assembly
            builder.ApplyConfigurationsFromAssembly(typeof(StudentCrmDbContext).Assembly);
        }
    }
}

