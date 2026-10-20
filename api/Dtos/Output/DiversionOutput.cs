using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace api.Dtos.Output;

// A bus line's path around a disruption (a journey_path_version of kind 'diversion')
public partial class DiversionOutput
{
    // the diversion's journey_path_version id
    public Guid Id { get; set; }

    public Guid FkJourneyId { get; set; }

    public Guid FkDisruptionId { get; set; }

    public string? RouteId { get; set; }

    public string? Name { get; set; }

    public string Direction { get; set; } = null!;

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    // the roads taken around the disruption, null when the line was only shortened
    public MultiLineString? Detour { get; set; }

    // planned stops the bus doesn't serve during the diversion, in path order
    public List<Guid> SkippedStopIds { get; set; } = new List<Guid>();
}
