using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class UsingJourneyMapper
{
    // table class to dto output mapping
    public partial UsingJourneyOutput toOutput(UsingJourney usingJourney);
    public partial IEnumerable<UsingJourneyOutput> toOutputList(IEnumerable<UsingJourney> usingJourneys);

    // dto input to table class mapping
    public partial UsingJourney toEntity(UsingJourneyInput input);
    // update entity with dto input mapping
    public partial void updateEntity(UsingJourneyInput input, UsingJourney entity);
}
