using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Domain.Entities;


namespace StudentCrm.Persistence.Configuration
{
    public class CoordinatorUserConfiguration : IEntityTypeConfiguration<CoordinatorUser>
    {
        public void Configure(EntityTypeBuilder<CoordinatorUser> builder)
        {
            builder.ToTable("Coordinators");

            builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Department).HasMaxLength(100);

            builder.HasOne(x => x.AppUser)
                .WithOne()
                .HasForeignKey<CoordinatorUser>(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AppUserId).IsUnique();
            builder.Property(x => x.CoordinatorNo).HasMaxLength(30);

            builder.HasIndex(x => x.CoordinatorNo)
                .IsUnique()
                .HasFilter("[CoordinatorNo] IS NOT NULL");
        }
    }
}
