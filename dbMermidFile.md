erDiagram
    vehicle_type ||--o{ vehicle : "has"
    vehicle_type ||--o{ stop : "served by"
    vehicle_type ||--o{ journey : "operates"
    journey ||--o{ journey_path_version : "versioned path"
    disruption |o--o{ journey_path_version : "causes diversion"
    journey_path_version ||--o{ journey_path : "ordered stops"
    stop ||--o{ journey_path : "used in"
    journey_path ||--o{ journey_path_time_plan : "planned times"
    journey ||--o{ using_journey : "assigned"
    vehicle ||--o{ using_journey : "drives"
    using_journey ||--o{ using_journey_log : "actual arrivals"
    stop ||--o{ using_journey_log : "arrived at"
    disruption ||--o{ disruption_stop : "blocks stop"
    stop ||--o{ disruption_stop : "blocked"
    disruption ||--o{ disruption_segment : "blocks segment"
    stop ||--o{ disruption_segment : "from / to"

    vehicle_type {
        uuid id PK
        varchar(100) name
    }

    vehicle {
        uuid id PK
        varchar(100) vehicle_number
        uuid fk_vehicle_type_id FK
    }

    journey {
        uuid id PK
        varchar(50) route_id
        varchar(50) name
        varchar(20) status
        varchar(20) direction
        uuid fk_vehicle_type_id FK
    }

    stop {
        uuid id PK
        varchar(200) name
        double_precision latitude
        double_precision longitude
        uuid fk_vehicle_type_id FK
    }

    disruption {
        uuid id PK
        varchar(50) cause
        text description
        geometry(Geometry) geometry
        timestamptz valid_from
        timestamptz valid_to
        timestamptz created_at
    }

    disruption_stop {
        uuid id PK
        uuid fk_disruption_id FK
        uuid fk_stop_id FK
    }

    disruption_segment {
        uuid id PK
        uuid fk_disruption_id FK
        uuid fk_from_stop_id FK
        uuid fk_to_stop_id FK
    }

    journey_path_version {
        uuid id PK
        uuid fk_journey_id FK
        int version_no
        varchar(20) kind
        timestamptz valid_from
        timestamptz valid_to
        uuid fk_disruption_id FK
        geometry(MultiLineString) detour
        timestamptz created_at
    }

    journey_path {
        uuid id PK
        uuid fk_journey_path_version_id FK
        uuid fk_stop_id FK
        int order
    }

    journey_path_time_plan {
        uuid id PK
        uuid fk_journey_path_id FK
        timestamptz planned_time
    }

    using_journey {
        uuid id PK
        uuid fk_journey_id FK
        uuid fk_vehicle_id FK
    }

    using_journey_log {
        uuid id PK
        uuid fk_using_journey_id FK
        uuid fk_stop_id FK
        timestamptz timestamp
    }
