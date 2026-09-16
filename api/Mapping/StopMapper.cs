using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class StopMapper
{
    // table class to dto output mapping
    public partial StopOutput toOutput(Stop stop);
    public partial IEnumerable<StopOutput> toOutputList(IEnumerable<Stop> stops);

    // dto input to table class mapping
    public partial Stop toEntity(StopInput input);
    // update entity with dto input mapping
    public partial void updateEntity(StopInput input, Stop entity);
}
