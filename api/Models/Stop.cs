using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Stop
{
    public Guid Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public Guid FkVehicleTypeId { get; set; }

    public virtual VehicleType FkVehicleType { get; set; } = null!;

    public virtual ICollection<JourneyPath> JourneyPaths { get; set; } = new List<JourneyPath>();

    public virtual ICollection<UsingJourneyLog> UsingJourneyLogs { get; set; } = new List<UsingJourneyLog>();
}
