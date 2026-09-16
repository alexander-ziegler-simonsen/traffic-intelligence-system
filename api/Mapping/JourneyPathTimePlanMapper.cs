using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class JourneyPathTimePlanMapper
{
    // table class to dto output mapping
    public partial JourneyPathTimePlanOutput toOutput(JourneyPathTimePlan journeyPathTimePlan);
    public partial IEnumerable<JourneyPathTimePlanOutput> toOutputList(IEnumerable<JourneyPathTimePlan> journeyPathTimePlans);

    // dto input to table class mapping
    public partial JourneyPathTimePlan toEntity(JourneyPathTimePlanInput input);
    // update entity with dto input mapping
    public partial void updateEntity(JourneyPathTimePlanInput input, JourneyPathTimePlan entity);
}
