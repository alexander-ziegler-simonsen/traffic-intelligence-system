using Riok.Mapperly.Abstractions;
using TisApi.Data.Postgres.Entities;
using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Mappers;

[Mapper]
public partial class IncidentMapper
{
    [MapperIgnoreSource(nameof(Incident.Camera))]
    public partial IncidentDto ToDto(Incident incident);

    [MapperIgnoreTarget(nameof(Incident.Id))]
    [MapperIgnoreTarget(nameof(Incident.Camera))]
    [MapperIgnoreTarget(nameof(Incident.RecordedAt))]
    public partial Incident ToEntity(CreateIncidentRequest request);
}
