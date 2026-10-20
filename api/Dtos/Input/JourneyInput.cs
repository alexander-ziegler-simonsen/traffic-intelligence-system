using System;
using System.Collections.Generic;

namespace api.Dtos.Input;

public partial class JourneyInput
{
    public Guid Id { get; set; }

    // GTFS route_id, the same id the route lines on the map carry
    public string? RouteId { get; set; }

    // line name shown to passengers, e.g. "5C"
    public string? Name { get; set; }

    public string Status { get; set; } = null!;

    public string Direction { get; set; } = null!;

    public Guid FkVehicleTypeId { get; set; }
}
