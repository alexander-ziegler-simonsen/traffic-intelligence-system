using Riok.Mapperly.Abstractions;
using TisApi.Data.Postgres.Entities;
using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Mappers;

[Mapper]
public partial class CameraMapper
{
    [MapperIgnoreSource(nameof(Camera.Road))]
    [MapperIgnoreSource(nameof(Camera.Incidents))]
    public partial CameraDto ToDto(Camera camera);

    [MapperIgnoreTarget(nameof(Camera.Id))]
    [MapperIgnoreTarget(nameof(Camera.Road))]
    [MapperIgnoreTarget(nameof(Camera.Incidents))]
    public partial Camera ToEntity(CreateCameraRequest request);

    [MapperIgnoreTarget(nameof(Camera.Id))]
    [MapperIgnoreTarget(nameof(Camera.RoadId))]
    [MapperIgnoreTarget(nameof(Camera.Road))]
    [MapperIgnoreTarget(nameof(Camera.Incidents))]
    public partial void UpdateEntity(UpdateCameraRequest request, [MappingTarget] Camera camera);
}
