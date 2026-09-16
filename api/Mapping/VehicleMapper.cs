using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class VehicleMapper
{
    // table class to dto output mapping
    public partial VehicleOutput toOutput(Vehicle vehicle);
    public partial IEnumerable<VehicleOutput> toOutputList(IEnumerable<Vehicle> vehicles);

    // dto input to table class mapping
    public partial Vehicle toEntity(VehicleInput input);
    // update entity with dto input mapping
    public partial void updateEntity(VehicleInput input, Vehicle entity);
}
