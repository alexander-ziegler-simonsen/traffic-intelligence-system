-- Load the full OpenMapTiles process script that ships with tilemaker.
-- We extend it below with transit-specific layers without touching the original.
dofile(os.getenv("TILEMAKER_PROCESS") or "/tilemaker/process.lua")

-- Extend node_function with transit nodes
local _base_node = node_function
function node_function()
  _base_node()

  local highway = Find("highway")
  local railway = Find("railway")

  -- Traffic lights
  if highway == "traffic_signals" then
    Layer("traffic_signals", false)
    return
  end

  -- Train stations / halts / stops
  if railway == "station" or railway == "halt" or railway == "stop" then
    Layer("train_stations", false)
    Attribute("name",    Find("name"))
    Attribute("railway", railway)
    return
  end
end
