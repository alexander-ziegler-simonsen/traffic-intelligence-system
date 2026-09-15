// Traffic Intelligence System — MongoDB read database
// Runs via /docker-entrypoint-initdb.d/ as the root user.
//
// Single merged collection: incident_reports
// Each document is the full join of roads + cameras + incidents from Postgres.
// No extra lookups needed — road and camera data are embedded in every document.

const readDb = db.getSiblingDB(process.env.MONGO_INITDB_DATABASE || 'tis_read');

readDb.createCollection('incident_reports');

readDb.incident_reports.createIndex({ recorded_at: -1 });
readDb.incident_reports.createIndex({ severity: 1 });
readDb.incident_reports.createIndex({ 'road.name': 1 });
readDb.incident_reports.createIndex({ 'camera.label': 1 });

readDb.incident_reports.insertMany([
    {
        postgres_id: 1,
        type:        'congestion',
        severity:    3,
        recorded_at: new Date(Date.now() - 2 * 60 * 60 * 1000),
        camera: {
            id:        1,
            label:     'CAM-001',
            latitude:  55.672584,
            longitude: 12.559208,
            status:    'active',
        },
        road: {
            id:          1,
            name:        'Vesterbrogade',
            type:        'arterial',
            speed_limit: 50,
            city:        'Copenhagen',
        },
    },
    {
        postgres_id: 2,
        type:        'accident',
        severity:    5,
        recorded_at: new Date(Date.now() - 30 * 60 * 1000),
        camera: {
            id:        1,
            label:     'CAM-001',
            latitude:  55.672584,
            longitude: 12.559208,
            status:    'active',
        },
        road: {
            id:          1,
            name:        'Vesterbrogade',
            type:        'arterial',
            speed_limit: 50,
            city:        'Copenhagen',
        },
    },
    {
        postgres_id: 3,
        type:        'roadwork',
        severity:    2,
        recorded_at: new Date(Date.now() - 24 * 60 * 60 * 1000),
        camera: {
            id:        2,
            label:     'CAM-002',
            latitude:  55.671900,
            longitude: 12.556400,
            status:    'active',
        },
        road: {
            id:          1,
            name:        'Vesterbrogade',
            type:        'arterial',
            speed_limit: 50,
            city:        'Copenhagen',
        },
    },
    {
        postgres_id: 4,
        type:        'hazard',
        severity:    4,
        recorded_at: new Date(Date.now() - 15 * 60 * 1000),
        camera: {
            id:        4,
            label:     'CAM-004',
            latitude:  56.157100,
            longitude: 10.195400,
            status:    'active',
        },
        road: {
            id:          3,
            name:        'Åboulevard',
            type:        'arterial',
            speed_limit: 60,
            city:        'Aarhus',
        },
    },
]);
