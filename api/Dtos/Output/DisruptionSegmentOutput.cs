using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Output;

public partial class DisruptionSegmentOutput
{
    public Guid Id { get; set; }

    public Guid FkDisruptionId { get; set; }

    public Guid FkFromStopId { get; set; }

    public Guid FkToStopId { get; set; }

    public virtual Disruption FkDisruption { get; set; } = null!;

    public virtual Stop FkFromStop { get; set; } = null!;

    public virtual Stop FkToStop { get; set; } = null!;
}
