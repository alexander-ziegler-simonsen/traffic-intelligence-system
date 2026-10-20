using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using api.Models;

namespace api.Dtos.Output;

public partial class DisruptionOutput
{
    public Guid Id { get; set; }

    public string Cause { get; set; } = null!;

    public string? Description { get; set; }

    public Geometry? Geometry { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<DisruptionSegment> DisruptionSegments { get; set; } = new List<DisruptionSegment>();

    public virtual ICollection<DisruptionStop> DisruptionStops { get; set; } = new List<DisruptionStop>();

    public virtual ICollection<JourneyPathVersion> JourneyPathVersions { get; set; } = new List<JourneyPathVersion>();
}
