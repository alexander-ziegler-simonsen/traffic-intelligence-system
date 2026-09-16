using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Input;

public partial class UsingJourneyInput
{
    public Guid Id { get; set; }

    public Guid FkJourneyId { get; set; }

    public Guid FkVehicleId { get; set; }

    public virtual Journey FkJourney { get; set; } = null!;

    public virtual Vehicle FkVehicle { get; set; } = null!;

    public virtual ICollection<UsingJourneyLog> UsingJourneyLogs { get; set; } = new List<UsingJourneyLog>();
}
