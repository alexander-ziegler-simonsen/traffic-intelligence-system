using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Input;

public partial class JourneyInput
{
    public Guid Id { get; set; }

    public string Status { get; set; } = null!;

    public string Direction { get; set; } = null!;

    public Guid FkVehicleTypeId { get; set; }

    public virtual VehicleType FkVehicleType { get; set; } = null!;

    public virtual ICollection<JourneyPath> JourneyPaths { get; set; } = new List<JourneyPath>();

    public virtual ICollection<UsingJourney> UsingJourneys { get; set; } = new List<UsingJourney>();
}
