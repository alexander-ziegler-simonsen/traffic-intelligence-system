using System;
using System.Collections.Generic;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

using Riok.Mapperly.Abstractions;

namespace api.Mapping;

[Mapper]
public partial class JourneyPathMapper
{
    // table class to dto output mapping
    public partial JourneyPathOutput toOutput(JourneyPath journeyPath);
    public partial IEnumerable<JourneyPathOutput> toOutputList(IEnumerable<JourneyPath> journeyPaths);

    // dto input to table class mapping
    public partial JourneyPath toEntity(JourneyPathInput input);
    // update entity with dto input mapping
    public partial void updateEntity(JourneyPathInput input, JourneyPath entity);
}
