using Riok.Mapperly.Abstractions;
using TisApi.Data.Postgres.Entities;
using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Mappers;

[Mapper]
public partial class RoadMapper
{
    [MapperIgnoreSource(nameof(Road.Cameras))]
    public partial RoadDto ToDto(Road road);

    [MapperIgnoreTarget(nameof(Road.Id))]
    [MapperIgnoreTarget(nameof(Road.Cameras))]
    public partial Road ToEntity(CreateRoadRequest request);

    [MapperIgnoreTarget(nameof(Road.Id))]
    [MapperIgnoreTarget(nameof(Road.Cameras))]
    public partial void UpdateEntity(UpdateRoadRequest request, [MappingTarget] Road road);
}
