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
    public class UniStudentConfiguration : IEntityTypeConfiguration<UniStudent>
    {
        public void Configure(EntityTypeBuilder<UniStudent> builder)
        {
            builder.ToTable("Students");

            builder.Property(x => x.StudentNo)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Surname)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Phone)
                .HasMaxLength(30);

            builder.Property(x => x.Email)
                .HasMaxLength(256);

            builder.HasIndex(x => x.StudentNo).IsUnique();
        }
    }
}
