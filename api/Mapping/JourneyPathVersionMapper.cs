using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class JourneyPathVersionMapper
{
    // table class to dto output mapping
    // related rows have their own endpoints, and would loop back to the version in the JSON
    [MapperIgnoreSource(nameof(JourneyPathVersion.FkDisruption))]
    [MapperIgnoreSource(nameof(JourneyPathVersion.FkJourney))]
    [MapperIgnoreSource(nameof(JourneyPathVersion.JourneyPaths))]
    public partial JourneyPathVersionOutput toOutput(JourneyPathVersion journeyPathVersion);
    public partial IEnumerable<JourneyPathVersionOutput> toOutputList(IEnumerable<JourneyPathVersion> journeyPathVersions);

    // dto input to table class mapping
    [MapperIgnoreTarget(nameof(JourneyPathVersion.CreatedAt))]
    [MapperIgnoreTarget(nameof(JourneyPathVersion.FkDisruption))]
    [MapperIgnoreTarget(nameof(JourneyPathVersion.FkJourney))]
    [MapperIgnoreTarget(nameof(JourneyPathVersion.JourneyPaths))]
    public partial JourneyPathVersion toEntity(JourneyPathVersionInput input);
    // update entity with dto input mapping
    [MapperIgnoreTarget(nameof(JourneyPathVersion.CreatedAt))]
    [MapperIgnoreTarget(nameof(JourneyPathVersion.FkDisruption))]
    [MapperIgnoreTarget(nameof(JourneyPathVersion.FkJourney))]
    [MapperIgnoreTarget(nameof(JourneyPathVersion.JourneyPaths))]
    public partial void updateEntity(JourneyPathVersionInput input, JourneyPathVersion entity);
}
