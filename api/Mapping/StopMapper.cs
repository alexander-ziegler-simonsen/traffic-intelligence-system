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
    // related rows have their own endpoints, and would loop back to the stop in the JSON
    [MapperIgnoreSource(nameof(Stop.FkVehicleType))]
    [MapperIgnoreSource(nameof(Stop.DisruptionSegmentFkFromStops))]
    [MapperIgnoreSource(nameof(Stop.DisruptionSegmentFkToStops))]
    [MapperIgnoreSource(nameof(Stop.DisruptionStops))]
    [MapperIgnoreSource(nameof(Stop.JourneyPaths))]
    [MapperIgnoreSource(nameof(Stop.UsingJourneyLogs))]
    public partial StopOutput toOutput(Stop stop);
    public partial IEnumerable<StopOutput> toOutputList(IEnumerable<Stop> stops);

    // dto input to table class mapping
    [MapperIgnoreTarget(nameof(Stop.FkVehicleType))]
    [MapperIgnoreTarget(nameof(Stop.DisruptionSegmentFkFromStops))]
    [MapperIgnoreTarget(nameof(Stop.DisruptionSegmentFkToStops))]
    [MapperIgnoreTarget(nameof(Stop.DisruptionStops))]
    [MapperIgnoreTarget(nameof(Stop.JourneyPaths))]
    [MapperIgnoreTarget(nameof(Stop.UsingJourneyLogs))]
    public partial Stop toEntity(StopInput input);
    // update entity with dto input mapping
    [MapperIgnoreTarget(nameof(Stop.FkVehicleType))]
    [MapperIgnoreTarget(nameof(Stop.DisruptionSegmentFkFromStops))]
    [MapperIgnoreTarget(nameof(Stop.DisruptionSegmentFkToStops))]
    [MapperIgnoreTarget(nameof(Stop.DisruptionStops))]
    [MapperIgnoreTarget(nameof(Stop.JourneyPaths))]
    [MapperIgnoreTarget(nameof(Stop.UsingJourneyLogs))]
    public partial void updateEntity(StopInput input, Stop entity);
}
