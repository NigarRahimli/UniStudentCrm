using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;

namespace StudentCrm.Persistence.Configuration
{
    public class TeacherUserConfiguration : IEntityTypeConfiguration<TeacherUser>
    {
        public void Configure(EntityTypeBuilder<TeacherUser> builder)
        {
            builder.ToTable("Teachers");

            builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Email).HasMaxLength(256);

            builder.Property(x => x.StaffNo).IsRequired();
            builder.HasIndex(x => x.StaffNo).IsUnique();

            // Teacher -> AppUser (required 1-1)
            builder.HasOne(x => x.AppUser)
                .WithOne()
                .HasForeignKey<TeacherUser>(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AppUserId).IsUnique();
        }
    }
}
