using AssetManagement.Models.Enums;
using System.Text.Json.Serialization;

namespace AssetManagement.DTOs
{
    public record MaintenanceCreateDto(
        int AssetID, 
        string? Description, 
        DateTime? ScheduleDate);
    public record MaintenanceUpdateDto(
        string? Description, 
        DateTime? ScheduleDate, 
        DateTime? CompletedDate, 
        MaintenanceStatus Status);
    public record MaintenanceResponseDto(
        int MaintenanceID, 
        int AssetID,
        String AssetName,
        string? Description, 
        DateTime? ScheduleDate, 
        DateTime? CompletedDate,
        [property : JsonConverter(typeof(JsonStringEnumConverter))]
        MaintenanceStatus Status);
}
