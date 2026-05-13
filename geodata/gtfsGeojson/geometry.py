from .config import BOUNDS


def in_bounds(lon: float, lat: float) -> bool:
    min_lon, min_lat, max_lon, max_lat = BOUNDS
    return min_lon <= lon <= max_lon and min_lat <= lat <= max_lat


def dp_simplify(points: list[tuple[float, float]], tol: float) -> list[tuple[float, float]]:
    """Iterative Ramer-Douglas-Peucker line simplification."""
    n = len(points)
    if n <= 2:
        return list(points)

    keep = bytearray(n)
    keep[0] = keep[-1] = 1
    stack = [(0, n - 1)]

    while stack:
        s, e = stack.pop()
        if e - s < 2:
            continue
        x1, y1 = points[s]
        x2, y2 = points[e]
        dx, dy = x2 - x1, y2 - y1
        ll = (dx * dx + dy * dy) ** 0.5
        mx, mi = 0.0, s

        for i in range(s + 1, e):
            px, py = points[i]
            if ll == 0:
                d = ((px - x1) ** 2 + (py - y1) ** 2) ** 0.5
            else:
                d = abs(dy * px - dx * py + x2 * y1 - y2 * x1) / ll
            if d > mx:
                mx, mi = d, i

        if mx > tol:
            keep[mi] = 1
            stack.append((s, mi))
            stack.append((mi, e))

    return [points[i] for i in range(n) if keep[i]]
