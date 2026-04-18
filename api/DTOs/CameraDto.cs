namespace TisApi.DTOs;

public record CameraDto(int Id, int RoadId, string Label, decimal Latitude, decimal Longitude, string Status);
