using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace api.Models;

public partial class JourneyPathVersion
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

    public DateTime CreatedAt { get; set; }

    public virtual Disruption? FkDisruption { get; set; }

    public virtual Journey FkJourney { get; set; } = null!;

    public virtual ICollection<JourneyPath> JourneyPaths { get; set; } = new List<JourneyPath>();
}
