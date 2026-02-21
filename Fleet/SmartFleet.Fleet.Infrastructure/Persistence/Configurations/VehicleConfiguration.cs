using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFleet.Fleet.Domain.Entities;

namespace SmartFleet.Auth.Infrastructure.Persistence.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            // Table Name
            builder.ToTable("Vehicles");

            // Primary Key
            builder.HasKey(v => v.Id);

            // VIN: Must be unique and is the primary identifier in the real world
            builder.HasIndex(v => v.VIN).IsUnique();
            builder.Property(v => v.VIN)
                .IsRequired()
                .HasMaxLength(17); // Standard VIN length

            builder.Property(v => v.Make)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(v => v.Year)
                .IsRequired();

            // ImageUrl: Allow long paths for Azure Blob Storage/Azurite
            builder.Property(v => v.ImageUrl)
                .HasMaxLength(2048);

            // Audit property
            builder.Property(v => v.CreatedAt)
                .IsRequired();

            // Map Enum to string or int (Default is int, which is efficient)
            builder.Property(v => v.Status)
                .IsRequired();
        }
    }
}
