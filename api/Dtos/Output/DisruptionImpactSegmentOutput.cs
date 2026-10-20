using System;

namespace api.Dtos.Output;

// Two stops that follow each other on a journey path
public partial class DisruptionImpactSegmentOutput
{
    public Guid FkFromStopId { get; set; }

    public Guid FkToStopId { get; set; }
}
