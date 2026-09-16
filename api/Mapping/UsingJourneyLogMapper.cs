using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class UsingJourneyLogMapper
{
    // table class to dto output mapping
    public partial UsingJourneyLogOutput toOutput(UsingJourneyLog usingJourneyLog);
    public partial IEnumerable<UsingJourneyLogOutput> toOutputList(IEnumerable<UsingJourneyLog> usingJourneyLogs);

    // dto input to table class mapping
    public partial UsingJourneyLog toEntity(UsingJourneyLogInput input);
    // update entity with dto input mapping
    public partial void updateEntity(UsingJourneyLogInput input, UsingJourneyLog entity);
}
