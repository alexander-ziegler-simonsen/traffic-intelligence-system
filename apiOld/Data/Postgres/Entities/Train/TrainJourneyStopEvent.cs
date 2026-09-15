using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("train_journey_stop_events")]
public class TrainJourneyStopEvent
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_journey_id")]
    public Guid FkJourneyId { get; set; }

    [Column("fk_station_id")]
    public Guid FkStationId { get; set; }

    [Column("event_type")]
    [MaxLength(20)]
    public string EventType { get; set; } = string.Empty;

    [Column("passengers_boarding")]
    public int PassengersBoarding { get; set; }

    [Column("passengers_alighting")]
    public int PassengersAlighting { get; set; }

    [Column("passengers_on_train")]
    public int PassengersOnTrain { get; set; }

    [Column("scheduled_arrival")]
    public DateTimeOffset? ScheduledArrival { get; set; }

    [Column("actual_arrival")]
    public DateTimeOffset? ActualArrival { get; set; }

    [Column("scheduled_departure")]
    public DateTimeOffset? ScheduledDeparture { get; set; }

    [Column("actual_departure")]
    public DateTimeOffset? ActualDeparture { get; set; }

    [Column("occurred_at")]
    public DateTimeOffset OccurredAt { get; set; }

    public TrainJourney Journey { get; set; } = null!;
    public TrainStation Station { get; set; } = null!;
}
