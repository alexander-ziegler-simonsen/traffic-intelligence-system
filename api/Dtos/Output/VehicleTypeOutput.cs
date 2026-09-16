using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Output;

public partial class VehicleTypeOutput
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Journey> Journeys { get; set; } = new List<Journey>();

    public virtual ICollection<Stop> Stops { get; set; } = new List<Stop>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
