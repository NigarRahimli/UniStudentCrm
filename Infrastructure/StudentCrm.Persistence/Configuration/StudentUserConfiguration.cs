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
    public class StudentUserConfiguration : IEntityTypeConfiguration<StudentUser>
    {
        public void Configure(EntityTypeBuilder<StudentUser> builder)
        {
            builder.ToTable("Students");

            builder.Property(x => x.StudentNo).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Surname).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Phone).HasMaxLength(30);
            builder.Property(x => x.Email).HasMaxLength(256);

            builder.HasIndex(x => x.StudentNo).IsUnique();

            // Student -> AppUser (optional 1-1)
            builder.HasOne(x => x.AppUser)
                .WithOne()
                .HasForeignKey<StudentUser>(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AppUserId)
                .IsUnique()
                .HasFilter("[AppUserId] IS NOT NULL");
        }
    }
}
