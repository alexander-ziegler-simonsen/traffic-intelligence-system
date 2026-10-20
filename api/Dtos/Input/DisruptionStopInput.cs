using System;
using System.Collections.Generic;
using api.Models;

namespace api.Dtos.Input;

public partial class DisruptionStopInput
{
    public Guid Id { get; set; }

    public Guid FkDisruptionId { get; set; }

    public Guid FkStopId { get; set; }

    public virtual Disruption FkDisruption { get; set; } = null!;

    public virtual Stop FkStop { get; set; } = null!;
}
