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
    public partial JourneyPathVersionOutput toOutput(JourneyPathVersion journeyPathVersion);
    public partial IEnumerable<JourneyPathVersionOutput> toOutputList(IEnumerable<JourneyPathVersion> journeyPathVersions);

    // dto input to table class mapping
    [MapperIgnoreTarget(nameof(JourneyPathVersion.CreatedAt))]
    public partial JourneyPathVersion toEntity(JourneyPathVersionInput input);
    // update entity with dto input mapping
    [MapperIgnoreTarget(nameof(JourneyPathVersion.CreatedAt))]
    public partial void updateEntity(JourneyPathVersionInput input, JourneyPathVersion entity);
}
