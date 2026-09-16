using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class JourneyMapper
{
    // table class to dto output mapping
    public partial JourneyOutput toOutput(Journey journey);
    public partial IEnumerable<JourneyOutput> toOutputList(IEnumerable<Journey> journeys);

    // dto input to table class mapping
    public partial Journey toEntity(JourneyInput input);
    // update entity with dto input mapping
    public partial void updateEntity(JourneyInput input, Journey entity);
}
