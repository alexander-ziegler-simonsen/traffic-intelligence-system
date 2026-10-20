using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Journey
{
    public Guid Id { get; set; }

    public string Status { get; set; } = null!;

    public string Direction { get; set; } = null!;

    public Guid FkVehicleTypeId { get; set; }

    public virtual VehicleType FkVehicleType { get; set; } = null!;

    public virtual ICollection<JourneyPathVersion> JourneyPathVersions { get; set; } = new List<JourneyPathVersion>();

    public virtual ICollection<UsingJourney> UsingJourneys { get; set; } = new List<UsingJourney>();
}
