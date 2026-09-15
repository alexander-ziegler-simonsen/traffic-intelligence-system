using System;
using System.Collections.Generic;

namespace api.Models;

public partial class JourneyPathTimePlan
{
    public Guid Id { get; set; }

    public Guid FkJourneyPathId { get; set; }

    public DateTime PlannedTime { get; set; }

    public virtual JourneyPath FkJourneyPath { get; set; } = null!;
}
