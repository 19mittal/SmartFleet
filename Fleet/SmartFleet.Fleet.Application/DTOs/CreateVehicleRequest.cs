using System.ComponentModel.DataAnnotations;

namespace SmartFleet.Fleet.Application.DTOs
{
    public record CreateVehicleRequest(
     [Required]
    [StringLength(17, MinimumLength = 11)]
    string VIN,

     [Required]
    [MaxLength(50)]
    string Make,

     [Required]
    [MaxLength(50)]
    string Model,

     [Required]
    [Range(1900, 2100)]
    int Year,

     [MaxLength(2048)]
    string? ImageUrl
 );
}
