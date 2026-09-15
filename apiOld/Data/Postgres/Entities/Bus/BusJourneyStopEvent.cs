using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("bus_journey_stop_events")]
public class BusJourneyStopEvent
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_journey_id")]
    public Guid FkJourneyId { get; set; }

    [Column("fk_bus_stop_id")]
    public Guid FkBusStopId { get; set; }

    [Column("event_type")]
    [MaxLength(20)]
    public string EventType { get; set; } = string.Empty;

    [Column("passengers_boarding")]
    public int PassengersBoarding { get; set; }

    [Column("passengers_alighting")]
    public int PassengersAlighting { get; set; }

    [Column("passengers_on_bus")]
    public int PassengersOnBus { get; set; }

    [Column("occurred_at")]
    public DateTimeOffset OccurredAt { get; set; }

    public BusJourney Journey { get; set; } = null!;
    public BusStop BusStop { get; set; } = null!;
}
