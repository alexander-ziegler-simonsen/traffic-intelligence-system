#!/bin/sh
set -e

# Start Redis temporarily as a daemon to allow seeding
redis-server --requirepass "$REDIS_PASSWORD" --daemonize yes --logfile ""

# Wait until it accepts connections
until redis-cli -a "$REDIS_PASSWORD" ping 2>/dev/null | grep -q PONG; do
  sleep 0.2
done

# Seed live tracking data
redis-cli -a "$REDIS_PASSWORD" <<'EOF'
HSET live:camera:1 vehicle_count 87  avg_speed_kmh 38 last_updated 1713196800
HSET live:camera:2 vehicle_count 14  avg_speed_kmh 51 last_updated 1713196800
HSET live:camera:3 vehicle_count 0   avg_speed_kmh 0  last_updated 1713196800
HSET live:camera:4 vehicle_count 120 avg_speed_kmh 82 last_updated 1713196800

HSET live:incident:1 status open   type congestion severity 3
HSET live:incident:2 status open   type accident   severity 5
HSET live:incident:3 status closed type roadwork   severity 2
HSET live:incident:4 status open   type hazard     severity 4

SADD live:road:1:cameras 1 2
SADD live:road:2:cameras 3
SADD live:road:3:cameras 4

EXPIRE live:incident:1 86400
EXPIRE live:incident:2 86400
EXPIRE live:incident:4 86400
EOF

# Shut down the daemon and re-launch as PID 1 so Docker signals work correctly
redis-cli -a "$REDIS_PASSWORD" shutdown nosave 2>/dev/null || true
sleep 0.5

exec redis-server --requirepass "$REDIS_PASSWORD"
