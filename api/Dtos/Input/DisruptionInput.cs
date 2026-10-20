using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using api.Models;

namespace api.Dtos.Input;

public partial class DisruptionInput
{
    public Guid Id { get; set; }

    public string Cause { get; set; } = null!;

    public string? Description { get; set; }

    // GeoJSON geometry from the map, e.g. { "type": "Polygon", "coordinates": [...] }
    public Geometry? Geometry { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public virtual ICollection<DisruptionSegment> DisruptionSegments { get; set; } = new List<DisruptionSegment>();

    public virtual ICollection<DisruptionStop> DisruptionStops { get; set; } = new List<DisruptionStop>();

    public virtual ICollection<JourneyPathVersion> JourneyPathVersions { get; set; } = new List<JourneyPathVersion>();
}
