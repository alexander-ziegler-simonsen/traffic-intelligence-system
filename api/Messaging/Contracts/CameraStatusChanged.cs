namespace TisApi.Messaging.Contracts;

public record CameraStatusChanged(int CameraId, string OldStatus, string NewStatus);
