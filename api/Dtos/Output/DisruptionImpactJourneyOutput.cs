using System;

namespace api.Dtos.Output;

// A bus line that would have to be rerouted
public partial class DisruptionImpactJourneyOutput
{
    public Guid Id { get; set; }

    public string? RouteId { get; set; }

    public string? Name { get; set; }

    public string Direction { get; set; } = null!;
}
