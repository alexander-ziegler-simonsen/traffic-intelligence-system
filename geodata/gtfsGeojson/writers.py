import json
import os


def write_route_geojson(features_by_mode: dict[str, list], out_dir: str) -> None:
    print("Writing GeoJSON files ...")
    for mode, features in sorted(features_by_mode.items()):
        path = f"{out_dir}/{mode}-routes.geojson"
        with open(path, "w", encoding="utf-8") as f:
            json.dump({"type": "FeatureCollection", "features": features}, f, separators=(",", ":"))
        size_kb = os.path.getsize(path) / 1024
        print(f"  {mode:12s}  {len(features):5d} shapes  →  {path}  ({size_kb:,.0f} KB)")


def write_route_index(route_index: dict[str, list], client_gtfs_dir: str) -> None:
    print("Writing route-index.json ...")
    sorted_index = {
        mode: sorted(routes, key=lambda r: r["name"].lower())
        for mode, routes in route_index.items()
    }
    index_path = f"{client_gtfs_dir}/route-index.json"
    with open(index_path, "w", encoding="utf-8") as f:
        json.dump(sorted_index, f, separators=(",", ":"), ensure_ascii=False)
    total_routes = sum(len(v) for v in sorted_index.values())
    print(f"  {total_routes} unique routes → {index_path}")


def write_stop_geojson(stop_features: dict[str, list], out_dir: str) -> None:
    print("Writing stop GeoJSON files ...")
    for mode, features in sorted(stop_features.items()):
        path = f"{out_dir}/{mode}-stops.geojson"
        with open(path, "w", encoding="utf-8") as f:
            json.dump({"type": "FeatureCollection", "features": features}, f, separators=(",", ":"))
        size_kb = os.path.getsize(path) / 1024
        print(f"  {mode:12s}  {len(features):5d} stops   →  {path}  ({size_kb:,.0f} KB)")
