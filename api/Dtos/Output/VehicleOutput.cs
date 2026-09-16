using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Output;

public partial class VehicleOutput
{
    public Guid Id { get; set; }

    public string VehicleNumber { get; set; } = null!;

    public Guid FkVehicleTypeId { get; set; }

    public virtual VehicleType FkVehicleType { get; set; } = null!;

    public virtual ICollection<UsingJourney> UsingJourneys { get; set; } = new List<UsingJourney>();
}
