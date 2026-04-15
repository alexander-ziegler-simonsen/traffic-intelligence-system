-- Traffic Intelligence System — PostgreSQL source-of-truth schema
--
-- FK chain:  roads ←── cameras ←── incidents
-- A road has many cameras; a camera records many incidents.

CREATE TABLE roads (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL,
    type        VARCHAR(20)  NOT NULL CHECK (type IN ('motorway', 'arterial', 'local')),
    speed_limit SMALLINT     NOT NULL CHECK (speed_limit > 0),
    city        VARCHAR(100) NOT NULL
);

CREATE TABLE cameras (
    id        SERIAL PRIMARY KEY,
    road_id   INT          NOT NULL REFERENCES roads (id) ON DELETE CASCADE,
    label     VARCHAR(20)  NOT NULL UNIQUE,
    latitude  NUMERIC(9,6) NOT NULL,
    longitude NUMERIC(9,6) NOT NULL,
    status    VARCHAR(10)  NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'inactive'))
);

CREATE TABLE incidents (
    id          SERIAL PRIMARY KEY,
    camera_id   INT         NOT NULL REFERENCES cameras (id) ON DELETE CASCADE,
    type        VARCHAR(20) NOT NULL CHECK (type IN ('accident', 'congestion', 'roadwork', 'hazard')),
    severity    SMALLINT    NOT NULL CHECK (severity BETWEEN 1 AND 5),
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Seed
INSERT INTO roads (name, type, speed_limit, city) VALUES
    ('Vesterbrogade',  'arterial', 50, 'Copenhagen'),
    ('Ring 3',         'motorway', 90, 'Copenhagen'),
    ('Åboulevard',     'arterial', 60, 'Aarhus');

INSERT INTO cameras (road_id, label, latitude, longitude, status) VALUES
    (1, 'CAM-001', 55.672584, 12.559208, 'active'),
    (1, 'CAM-002', 55.671900, 12.556400, 'active'),
    (2, 'CAM-003', 55.683000, 12.489200, 'inactive'),
    (3, 'CAM-004', 56.157100, 10.195400, 'active');

INSERT INTO incidents (camera_id, type, severity, recorded_at) VALUES
    (1, 'congestion', 3, now() - interval '2 hours'),
    (1, 'accident',   5, now() - interval '30 minutes'),
    (2, 'roadwork',   2, now() - interval '1 day'),
    (4, 'hazard',     4, now() - interval '15 minutes');

-- Convenience view — pre-joined for quick queries
CREATE VIEW incident_full AS
SELECT
    i.id            AS incident_id,
    i.type,
    i.severity,
    i.recorded_at,
    c.id            AS camera_id,
    c.label         AS camera_label,
    c.latitude,
    c.longitude,
    c.status        AS camera_status,
    r.id            AS road_id,
    r.name          AS road_name,
    r.type          AS road_type,
    r.speed_limit,
    r.city
FROM incidents i
JOIN cameras   c ON c.id = i.camera_id
JOIN roads     r ON r.id = c.road_id;
