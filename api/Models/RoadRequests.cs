using System.ComponentModel.DataAnnotations;

namespace TisApi.Models;

public record CreateRoadRequest(
    [Required][MaxLength(100)] string Name,
    [Required][RegularExpression("motorway|arterial|local")] string Type,
    [Range(1, 999)] short SpeedLimit,
    [Required][MaxLength(100)] string City
);

public record UpdateRoadRequest(
    [Required][MaxLength(100)] string Name,
    [Required][RegularExpression("motorway|arterial|local")] string Type,
    [Range(1, 999)] short SpeedLimit,
    [Required][MaxLength(100)] string City
);
