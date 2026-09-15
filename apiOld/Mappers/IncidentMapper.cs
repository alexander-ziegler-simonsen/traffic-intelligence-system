using Riok.Mapperly.Abstractions;
using TisApi.Data.Postgres.Entities;
using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Mappers;

[Mapper]
public partial class IncidentMapper
{
    public partial IncidentDto ToDto(Incident incident);

    [MapperIgnoreTarget(nameof(Incident.Id))]
    [MapperIgnoreTarget(nameof(Incident.ResolvedAt))]
    [MapperIgnoreTarget(nameof(Incident.IncidentEvents))]
    [MapperIgnoreTarget(nameof(Incident.RouteImpacts))]
    [MapperIgnoreTarget(nameof(Incident.WebhookDeliveries))]
    public partial Incident ToEntity(CreateIncidentRequest request);
}
