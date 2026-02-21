using SmartFleet.Fleet.Domain.Enums;

namespace SmartFleet.Fleet.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public string VIN { get; set; } = string.Empty; // Unique Fingerprint

        public string Make { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public VehicleStatus Status { get; set; } = VehicleStatus.Active;

        public DateTime? LastServiceDate { get; set; }

        public string? ImageUrl { get; set; } // Points to your Azurite/Azure Blob Storage path

        // As a lead, think ahead: 
        // We might need a property for CurrentMileage for maintenance predictions later
        public double CurrentMileage { get; set; }
    }
}
