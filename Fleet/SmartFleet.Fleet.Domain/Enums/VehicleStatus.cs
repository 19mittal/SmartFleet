namespace SmartFleet.Fleet.Domain.Enums
{
    public enum VehicleStatus
    {
        Active = 1,
        InService = 2,      // Currently in workshop
        MaintenanceDue = 3, // Flagged by system
        Retired = 4,
        Inactive = 5
    }
}
