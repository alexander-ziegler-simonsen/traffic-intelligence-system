-- Traffic Intelligence System — PostgreSQL schema
-- Matches TisPostgresContext entity definitions

-- Roads

CREATE TABLE roads (
    id          SERIAL       PRIMARY KEY,
    name        VARCHAR(100) NOT NULL,
    type        VARCHAR(20)  NOT NULL CHECK (type IN ('motorway', 'arterial', 'local')),
    speed_limit SMALLINT     NOT NULL CHECK (speed_limit > 0),
    city        VARCHAR(100) NOT NULL
);

CREATE TABLE cameras (
    id        SERIAL       PRIMARY KEY,
    road_id   INT          NOT NULL REFERENCES roads (id) ON DELETE CASCADE,
    label     VARCHAR(20)  NOT NULL UNIQUE,
    latitude  NUMERIC(9,6) NOT NULL,
    longitude NUMERIC(9,6) NOT NULL,
    status    VARCHAR(10)  NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'inactive'))
);

-- Incidents

CREATE TABLE incidents (
    id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    type             VARCHAR(50)  NOT NULL,
    status           VARCHAR(20)  NOT NULL,
    lat              DOUBLE PRECISION NOT NULL,
    lng              DOUBLE PRECISION NOT NULL,
    road_segment_id  VARCHAR(100) NOT NULL,
    description      VARCHAR(1000) NOT NULL,
    reported_at      TIMESTAMPTZ  NOT NULL DEFAULT now(),
    resolved_at      TIMESTAMPTZ
);

CREATE TABLE incident_events (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_incident_id  UUID        NOT NULL REFERENCES incidents (id) ON DELETE CASCADE,
    sequence_number INT         NOT NULL,
    event_type      VARCHAR(50) NOT NULL,
    payload         JSONB       NOT NULL,
    occurred_at     TIMESTAMPTZ NOT NULL
);

CREATE TABLE route_impacts (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_incident_id  UUID        NOT NULL REFERENCES incidents (id) ON DELETE CASCADE,
    line_id         VARCHAR(50) NOT NULL,
    line_name       VARCHAR(100) NOT NULL,
    impact_level    VARCHAR(20) NOT NULL,
    detected_at     TIMESTAMPTZ NOT NULL
);

CREATE TABLE reroute_decisions (
    id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_route_impact_id  UUID         NOT NULL REFERENCES route_impacts (id) ON DELETE CASCADE,
    detour_geometry     JSONB        NOT NULL,
    original_segment_id VARCHAR(100) NOT NULL,
    detour_via          VARCHAR(255) NOT NULL,
    decided_at          TIMESTAMPTZ  NOT NULL,
    revoked_at          TIMESTAMPTZ
);

-- Bus──

CREATE TABLE bus_routes (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    line_id          VARCHAR(50) NOT NULL,
    line_name        VARCHAR(100) NOT NULL,
    direction        VARCHAR(50) NOT NULL,
    capacity_per_bus INT         NOT NULL,
    status           VARCHAR(20) NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE bus_stops (
    id             UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_route_id    UUID         NOT NULL REFERENCES bus_routes (id) ON DELETE CASCADE,
    stop_name      VARCHAR(100) NOT NULL,
    osm_stop_id    VARCHAR(100) NOT NULL,
    sequence_order INT          NOT NULL,
    lat            DOUBLE PRECISION NOT NULL,
    lng            DOUBLE PRECISION NOT NULL
);

CREATE TABLE bus_route_assignments (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_route_id     UUID         NOT NULL REFERENCES bus_routes (id) ON DELETE CASCADE,
    bus_identifier  VARCHAR(100) NOT NULL,
    status          VARCHAR(20)  NOT NULL,
    assigned_at     TIMESTAMPTZ  NOT NULL,
    removed_at      TIMESTAMPTZ,
    removal_reason  VARCHAR(255)
);

CREATE TABLE bus_journeys (
    id                      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_route_id             UUID         NOT NULL REFERENCES bus_routes (id) ON DELETE CASCADE,
    fk_route_assignment_id  UUID         NOT NULL REFERENCES bus_route_assignments (id) ON DELETE RESTRICT,
    bus_identifier          VARCHAR(100) NOT NULL,
    started_at              TIMESTAMPTZ  NOT NULL,
    completed_at            TIMESTAMPTZ,
    status                  VARCHAR(20)  NOT NULL
);

CREATE TABLE bus_journey_stop_events (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_journey_id       UUID        NOT NULL REFERENCES bus_journeys (id) ON DELETE CASCADE,
    fk_bus_stop_id      UUID        NOT NULL REFERENCES bus_stops (id) ON DELETE RESTRICT,
    event_type          VARCHAR(20) NOT NULL,
    passengers_boarding INT         NOT NULL DEFAULT 0,
    passengers_alighting INT        NOT NULL DEFAULT 0,
    passengers_on_bus   INT         NOT NULL DEFAULT 0,
    occurred_at         TIMESTAMPTZ NOT NULL
);

-- Train

CREATE TABLE train_routes (
    id                 UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    line_id            VARCHAR(50) NOT NULL,
    line_name          VARCHAR(100) NOT NULL,
    direction          VARCHAR(50) NOT NULL,
    train_type         VARCHAR(50) NOT NULL,
    capacity_per_train INT         NOT NULL,
    status             VARCHAR(20) NOT NULL,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE train_stations (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_route_id     UUID         NOT NULL REFERENCES train_routes (id) ON DELETE CASCADE,
    station_name    VARCHAR(100) NOT NULL,
    osm_station_id  VARCHAR(100) NOT NULL,
    sequence_order  INT          NOT NULL,
    lat             DOUBLE PRECISION NOT NULL,
    lng             DOUBLE PRECISION NOT NULL,
    has_platform    BOOLEAN      NOT NULL DEFAULT false,
    platform_number VARCHAR(20)
);

CREATE TABLE train_route_assignments (
    id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_route_id      UUID         NOT NULL REFERENCES train_routes (id) ON DELETE CASCADE,
    train_identifier VARCHAR(100) NOT NULL,
    status           VARCHAR(20)  NOT NULL,
    assigned_at      TIMESTAMPTZ  NOT NULL,
    removed_at       TIMESTAMPTZ,
    removal_reason   VARCHAR(255)
);

CREATE TABLE train_journeys (
    id                      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_route_id             UUID         NOT NULL REFERENCES train_routes (id) ON DELETE CASCADE,
    fk_route_assignment_id  UUID         NOT NULL REFERENCES train_route_assignments (id) ON DELETE RESTRICT,
    train_identifier        VARCHAR(100) NOT NULL,
    started_at              TIMESTAMPTZ  NOT NULL,
    completed_at            TIMESTAMPTZ,
    status                  VARCHAR(20)  NOT NULL,
    delay_minutes           INT          NOT NULL DEFAULT 0
);

CREATE TABLE train_journey_stop_events (
    id                   UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_journey_id        UUID        NOT NULL REFERENCES train_journeys (id) ON DELETE CASCADE,
    fk_station_id        UUID        NOT NULL REFERENCES train_stations (id) ON DELETE RESTRICT,
    event_type           VARCHAR(20) NOT NULL,
    passengers_boarding  INT         NOT NULL DEFAULT 0,
    passengers_alighting INT         NOT NULL DEFAULT 0,
    passengers_on_train  INT         NOT NULL DEFAULT 0,
    scheduled_arrival    TIMESTAMPTZ,
    actual_arrival       TIMESTAMPTZ,
    scheduled_departure  TIMESTAMPTZ,
    actual_departure     TIMESTAMPTZ,
    occurred_at          TIMESTAMPTZ NOT NULL
);

-- Traffic Lights

CREATE TABLE traffic_lights (
    id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    osm_node_id          VARCHAR(100) NOT NULL,
    lat                  DOUBLE PRECISION NOT NULL,
    lng                  DOUBLE PRECISION NOT NULL,
    road_segment_id      VARCHAR(100) NOT NULL,
    normal_green_seconds INT          NOT NULL,
    normal_red_seconds   INT          NOT NULL,
    status               VARCHAR(20)  NOT NULL,
    created_at           TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE traffic_light_phases (
    id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_traffic_light_id  UUID         NOT NULL REFERENCES traffic_lights (id) ON DELETE CASCADE,
    phase_name           VARCHAR(100) NOT NULL,
    direction            VARCHAR(50)  NOT NULL,
    normal_green_seconds INT          NOT NULL,
    sequence_order       INT          NOT NULL
);

CREATE TABLE traffic_light_override_events (
    id                       UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_traffic_light_id      UUID        NOT NULL REFERENCES traffic_lights (id) ON DELETE CASCADE,
    fk_phase_id              UUID        NOT NULL REFERENCES traffic_light_phases (id) ON DELETE RESTRICT,
    trigger_type             VARCHAR(50) NOT NULL,
    trigger_detail           JSONB       NOT NULL,
    override_duration_seconds INT        NOT NULL,
    started_at               TIMESTAMPTZ NOT NULL,
    expires_at               TIMESTAMPTZ NOT NULL,
    returned_to_normal_at    TIMESTAMPTZ
);

-- Subscribers

CREATE TABLE subscribers (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    webhook_url VARCHAR(500) NOT NULL,
    secret_key  VARCHAR(255) NOT NULL,
    line_ids    VARCHAR[]    NOT NULL DEFAULT '{}',
    status      VARCHAR(20)  NOT NULL,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at  TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE webhook_deliveries (
    id                UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    fk_incident_id    UUID         REFERENCES incidents (id) ON DELETE SET NULL,
    fk_subscriber_id  UUID         NOT NULL REFERENCES subscribers (id) ON DELETE CASCADE,
    event_type        VARCHAR(50)  NOT NULL,
    payload           JSONB        NOT NULL,
    status            VARCHAR(20)  NOT NULL,
    attempt_number    INT          NOT NULL DEFAULT 1,
    http_status_code  INT,
    error_message     VARCHAR(1000),
    attempted_at      TIMESTAMPTZ,
    next_retry_at     TIMESTAMPTZ
);

-- Simulation

CREATE TABLE simulator_configs (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    entity_type      VARCHAR(50) NOT NULL,
    entity_id        VARCHAR(100) NOT NULL,
    route_id         UUID        NOT NULL,
    tick_interval_ms INT         NOT NULL,
    status           VARCHAR(20) NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Seed─

INSERT INTO roads (name, type, speed_limit, city) VALUES
    ('Vesterbrogade', 'arterial', 50, 'Copenhagen'),
    ('Ring 3',        'motorway', 90, 'Copenhagen'),
    ('Åboulevard',    'arterial', 60, 'Aarhus');

INSERT INTO cameras (road_id, label, latitude, longitude, status) VALUES
    (1, 'CAM-001', 55.672584, 12.559208, 'active'),
    (1, 'CAM-002', 55.671900, 12.556400, 'active'),
    (2, 'CAM-003', 55.683000, 12.489200, 'inactive'),
    (3, 'CAM-004', 56.157100, 10.195400, 'active');

INSERT INTO incidents (type, status, lat, lng, road_segment_id, description, reported_at) VALUES
    ('congestion', 'active',   55.672584, 12.559208, 'vesterbrogade-001', 'Heavy congestion near city centre', now() - interval '2 hours'),
    ('accident',   'active',   55.671900, 12.556400, 'vesterbrogade-002', 'Two-vehicle collision, lane blocked',  now() - interval '30 minutes'),
    ('roadwork',   'resolved', 55.683000, 12.489200, 'ring3-001',         'Scheduled resurfacing works',         now() - interval '1 day'),
    ('hazard',     'active',   56.157100, 10.195400, 'aaboulevard-001',   'Debris on road surface',              now() - interval '15 minutes');
