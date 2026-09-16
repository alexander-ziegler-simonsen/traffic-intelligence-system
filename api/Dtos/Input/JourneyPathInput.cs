using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Input;

public partial class JourneyPathInput
{
    public Guid Id { get; set; }

    public Guid FkJourneyId { get; set; }

    public Guid FkStopId { get; set; }

    public int Order { get; set; }

    public virtual Journey FkJourney { get; set; } = null!;

    public virtual Stop FkStop { get; set; } = null!;

    public virtual ICollection<JourneyPathTimePlan> JourneyPathTimePlans { get; set; } = new List<JourneyPathTimePlan>();
}
