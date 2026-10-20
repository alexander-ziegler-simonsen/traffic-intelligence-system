using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class DisruptionMapper
{
    // table class to dto output mapping
    public partial DisruptionOutput toOutput(Disruption disruption);
    public partial IEnumerable<DisruptionOutput> toOutputList(IEnumerable<Disruption> disruptions);

    // dto input to table class mapping
    [MapperIgnoreTarget(nameof(Disruption.CreatedAt))]
    public partial Disruption toEntity(DisruptionInput input);
    // update entity with dto input mapping
    [MapperIgnoreTarget(nameof(Disruption.CreatedAt))]
    public partial void updateEntity(DisruptionInput input, Disruption entity);
}
