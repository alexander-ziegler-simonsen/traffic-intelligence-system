using Riok.Mapperly.Abstractions;
using TisApi.Data.Redis.Models;
using TisApi.DTOs;

namespace TisApi.Mappers;

[Mapper]
public partial class LiveMapper
{
    public partial CameraLiveDto ToDto(CameraLive camera);
    public partial IncidentLiveDto ToDto(IncidentLive incident);
}
