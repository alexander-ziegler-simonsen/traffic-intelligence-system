CREATE TABLE vehicle_type (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    "name"        VARCHAR(100) NOT NULL
);

CREATE TABLE vehicle (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    vehicle_number        VARCHAR(100) NOT NULL,
    fk_vehicle_type_id    UUID         NOT NULL REFERENCES vehicle_type (id) 
);

CREATE TABLE journey (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    "status"     VARCHAR(20)  NOT NULL,
    direction  VARCHAR(20)  NOT NULL,
    fk_vehicle_type_id  UUID         NOT NULL REFERENCES vehicle_type (id)
);

CREATE TABLE "stop" (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    latitude    DOUBLE PRECISION NOT NULL,
    longitude   DOUBLE PRECISION NOT NULL,
    fk_vehicle_type_id  UUID         NOT NULL REFERENCES vehicle_type (id)
);

CREATE TABLE journey_path (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    fk_journey_id  UUID         NOT NULL REFERENCES "journey" (id),
    fk_stop_id   UUID         NOT NULL REFERENCES "stop" (id),
    "order" INT NOT NULL
);

CREATE TABLE journey_path_time_plan (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    fk_journey_path_id  UUID         NOT NULL REFERENCES journey_path (id),
    planned_time TIMESTAMPTZ NOT NULL
);

CREATE TABLE using_journey (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    fk_journey_id  UUID         NOT NULL REFERENCES "journey" (id),
    fk_vehicle_id  UUID         NOT NULL REFERENCES vehicle (id)
);

CREATE TABLE using_journey_log (
    id          UUID         PRIMARY KEY DEFAULT uuidv7(),
    fk_using_journey_id  UUID         NOT NULL REFERENCES using_journey (id),
    fk_stop_id UUID         NOT NULL REFERENCES "stop" (id),
    timestamp TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- seeding

BEGIN;

DO $$
DECLARE
    bus_type_id     uuid := '0195aaaa-0000-7000-8000-000000000001';
    metro_type_id   uuid := '0195aaaa-0000-7000-8000-000000000002';
    train_type_id   uuid := '0195aaaa-0000-7000-8000-000000000003';

    bus_vehicle_1   uuid := '0195bbbb-0000-7000-8000-000000000001';
    bus_vehicle_2   uuid := '0195bbbb-0000-7000-8000-000000000002';
    metro_vehicle_1 uuid := '0195bbbb-0000-7000-8000-000000000003';
    train_vehicle_1 uuid := '0195bbbb-0000-7000-8000-000000000004';

    bus_stop_1      uuid := '0195cccc-0000-7000-8000-000000000001';
    bus_stop_2      uuid := '0195cccc-0000-7000-8000-000000000002';
    bus_stop_3      uuid := '0195cccc-0000-7000-8000-000000000003';
    metro_stop_1    uuid := '0195cccc-0000-7000-8000-000000000004';
    train_stop_1    uuid := '0195cccc-0000-7000-8000-000000000005';

    bus_journey_out   uuid := '0195dddd-0000-7000-8000-000000000001';
    bus_journey_in    uuid := '0195dddd-0000-7000-8000-000000000002';
    metro_journey_in  uuid := '0195dddd-0000-7000-8000-000000000003';
    train_journey_out uuid := '0195dddd-0000-7000-8000-000000000004';
BEGIN
    -- vehicle_type
    IF EXISTS (SELECT 1 FROM vehicle_type LIMIT 1) THEN
        RAISE EXCEPTION 'vehicle_type already has data — refusing to reseed';
    END IF;
    INSERT INTO vehicle_type (id, name) VALUES
        (bus_type_id,   'Bus'),
        (metro_type_id, 'Metro'),
        (train_type_id, 'Train');

    -- vehicle
    IF EXISTS (SELECT 1 FROM vehicle LIMIT 1) THEN
        RAISE EXCEPTION 'vehicle already has data — refusing to reseed';
    END IF;
    INSERT INTO vehicle (id, vehicle_number, fk_vehicle_type_id) VALUES
        (bus_vehicle_1,   'BUS-001',   bus_type_id),
        (bus_vehicle_2,   'BUS-002',   bus_type_id),
        (metro_vehicle_1, 'METRO-001', metro_type_id),
        (train_vehicle_1, 'TRAIN-001', train_type_id);

    -- stop
    IF EXISTS (SELECT 1 FROM stop LIMIT 1) THEN
        RAISE EXCEPTION 'stop already has data — refusing to reseed';
    END IF;
    INSERT INTO stop (id, latitude, longitude, fk_vehicle_type_id) VALUES
        (bus_stop_1,   40.7128, -74.0060, bus_type_id),
        (bus_stop_2,   40.7138, -74.0070, bus_type_id),
        (bus_stop_3,   40.7148, -74.0080, bus_type_id),
        (metro_stop_1, 40.7158, -74.0090, metro_type_id),
        (train_stop_1, 40.7168, -74.0100, train_type_id);

    -- journey
    IF EXISTS (SELECT 1 FROM journey LIMIT 1) THEN
        RAISE EXCEPTION 'journey already has data — refusing to reseed';
    END IF;
    INSERT INTO journey (id, status, direction, fk_vehicle_type_id) VALUES
        (bus_journey_out,   'active', 'outbound', bus_type_id),
        (bus_journey_in,    'active', 'inbound',  bus_type_id),
        (metro_journey_in,  'active', 'inbound',  metro_type_id),
        (train_journey_out, 'active', 'outbound', train_type_id);

    -- journey_path
    IF EXISTS (SELECT 1 FROM journey_path LIMIT 1) THEN
        RAISE EXCEPTION 'journey_path already has data — refusing to reseed';
    END IF;
    INSERT INTO journey_path (fk_journey_id, fk_stop_id, "order") VALUES
        (bus_journey_out,  bus_stop_1,   1),
        (bus_journey_out,  bus_stop_2,   2),
        (bus_journey_out,  bus_stop_3,   3),
        (bus_journey_in,   metro_stop_1, 1),
        (metro_journey_in, train_stop_1, 1);
END $$;

COMMIT;