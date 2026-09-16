using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class VehicleTypeMapper
{
    // table class to dto output mapping
    public partial VehicleTypeOutput toOutput(VehicleType vehicleType);
    public partial IEnumerable<VehicleTypeOutput> toOutputList(IEnumerable<VehicleType> vehicleTypes);

    // dto input to table class mapping
    public partial VehicleType toEntity(VehicleTypeInput input);
    // update entity with dto input mapping
    public partial void updateEntity(VehicleTypeInput input, VehicleType entity);
}
