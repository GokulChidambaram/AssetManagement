using AssetManagement.Models.Enums;
using System.Text.Json.Serialization;

namespace AssetManagement.DTOs
{
    public record IssueCreateDto(
        int AssetID, 
        int ReportedByUserID, 
        string? Description, 
        bool RequiresRepair);
    public record IssueUpdateDto(
        string? Description, 
        IssueStatus Status);

    public record IssueStatusUpdateDto(int Status, string? Description);
    public record IssueResponseDto(
        int IssueID, 
        int AssetID,
        string AssetName,
        int ReportedByUserID,
        string ReportedByName,
        string? Description, 
        DateTime ReportedDate,
        [property : JsonConverter(typeof(JsonStringEnumConverter))]
        IssueStatus Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        string CreatedBy,
        string UpdatedBy

		);
}
