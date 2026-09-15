namespace TisApi.Simulation;

public sealed class SimulatorState
{
    private volatile string _geojson = """{"type":"FeatureCollection","features":[]}""";

    public string GeoJson => _geojson;

    public void Update(string geojson) => _geojson = geojson;
}
