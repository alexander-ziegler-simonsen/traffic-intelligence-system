using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace api.Dtos.Input;

public partial class DisruptionInput
{
    public Guid Id { get; set; }

    public string Cause { get; set; } = null!;

    public string? Description { get; set; }

    // GeoJSON geometry from the map, e.g. { "type": "Polygon", "coordinates": [...] }
    // When set, the disruption's stops and segments are worked out from it (see DisruptionService)
    public Geometry? Geometry { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }
}
