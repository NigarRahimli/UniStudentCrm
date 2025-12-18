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
    public class SectionConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.ToTable("Sections");

            builder.Property(x => x.SectionCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(x => x.Course)
                .WithMany()                 // istəsən Course-a ICollection<Section> əlavə edib WithMany(c=>c.Sections) yazarsan
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Term)
                .WithMany()
                .HasForeignKey(x => x.TermId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Teacher)
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // eyni Course+Term daxilində SectionCode təkrarlanmasın (A,B,...)
            builder.HasIndex(x => new { x.CourseId, x.TermId, x.SectionCode }).IsUnique();
        }
    }
}
