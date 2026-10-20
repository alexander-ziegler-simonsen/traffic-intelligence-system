using System;
using System.Collections.Generic;

namespace api.Models;

public partial class JourneyPath
{
    public Guid Id { get; set; }

    public Guid FkJourneyPathVersionId { get; set; }

    public Guid FkStopId { get; set; }

    public int Order { get; set; }

    public virtual JourneyPathVersion FkJourneyPathVersion { get; set; } = null!;

    public virtual Stop FkStop { get; set; } = null!;

    public virtual ICollection<JourneyPathTimePlan> JourneyPathTimePlans { get; set; } = new List<JourneyPathTimePlan>();
}
