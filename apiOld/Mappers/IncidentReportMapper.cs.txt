using Riok.Mapperly.Abstractions;
using TisApi.Data.Mongo.Documents;
using TisApi.DTOs;

namespace TisApi.Mappers;

[Mapper]
public partial class IncidentReportMapper
{
    public partial IncidentReportDto ToDto(IncidentReport report);

    private partial CameraInfoDto ToCameraInfoDto(CameraInfo camera);
    private partial RoadInfoDto ToRoadInfoDto(RoadInfo road);
}
