using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Input;

public partial class UsingJourneyLogInput
{
    public Guid Id { get; set; }

    public Guid FkUsingJourneyId { get; set; }

    public Guid FkStopId { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Stop FkStop { get; set; } = null!;

    public virtual UsingJourney FkUsingJourney { get; set; } = null!;
}
