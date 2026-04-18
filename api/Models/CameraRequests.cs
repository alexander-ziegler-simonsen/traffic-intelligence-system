using System.ComponentModel.DataAnnotations;

namespace TisApi.Models;

public record CreateCameraRequest(
    int RoadId,
    [Required][MaxLength(20)] string Label,
    decimal Latitude,
    decimal Longitude,
    [RegularExpression("active|inactive")] string Status = "active"
);

public record UpdateCameraRequest(
    [Required][MaxLength(20)] string Label,
    decimal Latitude,
    decimal Longitude,
    [Required][RegularExpression("active|inactive")] string Status
);
