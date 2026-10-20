using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace api.Dtos.Input;

public partial class JourneyPathVersionInput
{
    public Guid Id { get; set; }

    public Guid FkJourneyId { get; set; }

    public int VersionNo { get; set; }

    public string Kind { get; set; } = null!;

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public Guid? FkDisruptionId { get; set; }

    // diversions only: the roads taken around the disruption, one line per detour
    public MultiLineString? Detour { get; set; }
}
