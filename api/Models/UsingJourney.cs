using System;
using System.Collections.Generic;

namespace api.Models;

public partial class UsingJourney
{
    public Guid Id { get; set; }

    public Guid FkJourneyId { get; set; }

    public Guid FkVehicleId { get; set; }

    public virtual Journey FkJourney { get; set; } = null!;

    public virtual Vehicle FkVehicle { get; set; } = null!;

    public virtual ICollection<UsingJourneyLog> UsingJourneyLogs { get; set; } = new List<UsingJourneyLog>();
}
