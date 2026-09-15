using System;
using System.Collections.Generic;

namespace api.Models;

public partial class UsingJourneyLog
{
    public Guid Id { get; set; }

    public Guid FkUsingJourneyId { get; set; }

    public Guid FkStopId { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Stop FkStop { get; set; } = null!;

    public virtual UsingJourney FkUsingJourney { get; set; } = null!;
}
