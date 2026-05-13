import math

from .config import _LON_SCALE, _M_PER_DEG_LAT


def deg_dist_to_m(d_deg: float) -> float:
    """Convert a Shapely .distance() result (degrees) to approximate metres.

    Takes the geometric mean of the lat/lon scales — error < 1% for Denmark.
    """
    return d_deg * math.sqrt(_LON_SCALE * _M_PER_DEG_LAT * _M_PER_DEG_LAT)


def haversine_m(a: tuple[float, float], b: tuple[float, float]) -> float:
    """Great-circle distance in metres between two (lon, lat) WGS84 points."""
    R = 6_371_000.0
    lon1, lat1 = map(math.radians, a)
    lon2, lat2 = map(math.radians, b)
    dlat, dlon = lat2 - lat1, lon2 - lon1
    h = math.sin(dlat / 2) ** 2 + math.cos(lat1) * math.cos(lat2) * math.sin(dlon / 2) ** 2
    return 2 * R * math.asin(math.sqrt(h))


def sample_points(
    coords: list[tuple[float, float]], interval_m: float
) -> list[tuple[float, float]]:
    """Return evenly-spaced sample points along a polyline (~interval_m apart).

    Uses an accumulating-odometer walk: emits a point each time the running
    distance counter reaches a multiple of interval_m. Always includes the
    first and last vertex of the polyline.
    """
    if len(coords) < 2:
        return list(coords)

    pts = [coords[0]]
    accum = 0.0

    for i in range(1, len(coords)):
        seg_len = haversine_m(coords[i - 1], coords[i])
        if seg_len == 0:
            continue

        accum += seg_len

        while accum >= interval_m:
            t = 1.0 - (accum - interval_m) / seg_len
            lon = coords[i - 1][0] + t * (coords[i][0] - coords[i - 1][0])
            lat = coords[i - 1][1] + t * (coords[i][1] - coords[i - 1][1])
            pts.append((round(lon, 7), round(lat, 7)))
            accum -= interval_m

    if pts[-1] != coords[-1]:
        pts.append(coords[-1])

    return pts
