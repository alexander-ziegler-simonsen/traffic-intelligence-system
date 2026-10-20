using System;
using System.Collections.Generic;

namespace api.Dtos.Output;

// The stops and route segments a disruption's shape covers
public partial class DisruptionImpactOutput
{
    public List<Guid> StopIds { get; set; } = new List<Guid>();

    public List<DisruptionImpactSegmentOutput> Segments { get; set; } = new List<DisruptionImpactSegmentOutput>();
}
