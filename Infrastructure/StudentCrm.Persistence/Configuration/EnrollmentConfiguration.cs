using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Configuration
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");

            builder.HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Section)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StudentId, x.SectionId }).IsUnique();

            builder.Property(x => x.TotalGrade).HasPrecision(5, 2);
            builder.HasCheckConstraint("CK_Enrollments_TotalGrade_Range",
                "[TotalGrade] IS NULL OR ([TotalGrade] >= 0 AND [TotalGrade] <= 100)");

            builder.Property(x => x.LetterGrade).HasMaxLength(2);   
        }
    }
}
