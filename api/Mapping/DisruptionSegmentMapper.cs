using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class DisruptionSegmentMapper
{
    // table class to dto output mapping
    public partial DisruptionSegmentOutput toOutput(DisruptionSegment disruptionSegment);
    public partial IEnumerable<DisruptionSegmentOutput> toOutputList(IEnumerable<DisruptionSegment> disruptionSegments);

    // dto input to table class mapping
    public partial DisruptionSegment toEntity(DisruptionSegmentInput input);
    // update entity with dto input mapping
    public partial void updateEntity(DisruptionSegmentInput input, DisruptionSegment entity);
}
