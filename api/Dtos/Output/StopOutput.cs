using System;
using System.Collections.Generic;

namespace api.Dtos.Output;

public partial class StopOutput
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public Guid FkVehicleTypeId { get; set; }
}
