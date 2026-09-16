using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Output;

public partial class JourneyPathTimePlanOutput
{
    public Guid Id { get; set; }

    public Guid FkJourneyPathId { get; set; }

    public DateTime PlannedTime { get; set; }

    public virtual JourneyPath FkJourneyPath { get; set; } = null!;
}
