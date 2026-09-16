using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Input;

public partial class JourneyPathTimePlanInput
{
    public Guid Id { get; set; }

    public Guid FkJourneyPathId { get; set; }

    public DateTime PlannedTime { get; set; }

    public virtual JourneyPath FkJourneyPath { get; set; } = null!;
}
