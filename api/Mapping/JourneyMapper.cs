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
    // related rows have their own endpoints, and would loop back to the journey in the JSON
    [MapperIgnoreSource(nameof(Journey.FkVehicleType))]
    [MapperIgnoreSource(nameof(Journey.JourneyPathVersions))]
    [MapperIgnoreSource(nameof(Journey.UsingJourneys))]
    public partial JourneyOutput toOutput(Journey journey);
    public partial IEnumerable<JourneyOutput> toOutputList(IEnumerable<Journey> journeys);

    // dto input to table class mapping
    [MapperIgnoreTarget(nameof(Journey.FkVehicleType))]
    [MapperIgnoreTarget(nameof(Journey.JourneyPathVersions))]
    [MapperIgnoreTarget(nameof(Journey.UsingJourneys))]
    public partial Journey toEntity(JourneyInput input);
    // update entity with dto input mapping
    [MapperIgnoreTarget(nameof(Journey.FkVehicleType))]
    [MapperIgnoreTarget(nameof(Journey.JourneyPathVersions))]
    [MapperIgnoreTarget(nameof(Journey.UsingJourneys))]
    public partial void updateEntity(JourneyInput input, Journey entity);
}
