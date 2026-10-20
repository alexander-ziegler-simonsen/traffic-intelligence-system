using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class DisruptionStopMapper
{
    // table class to dto output mapping
    public partial DisruptionStopOutput toOutput(DisruptionStop disruptionStop);
    public partial IEnumerable<DisruptionStopOutput> toOutputList(IEnumerable<DisruptionStop> disruptionStops);

    // dto input to table class mapping
    public partial DisruptionStop toEntity(DisruptionStopInput input);
    // update entity with dto input mapping
    public partial void updateEntity(DisruptionStopInput input, DisruptionStop entity);
}
