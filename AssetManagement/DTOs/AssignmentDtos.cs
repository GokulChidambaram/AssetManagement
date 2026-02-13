using AssetManagement.Models.Enums;

namespace AssetManagement.DTOs
{
    public record AssignAssetDto(
        int AssetID, int AssignedToUserID, 
        DateTime AssignedDate, 
        string? Location);
    public record ReturnAssetDto(
        int AssignmentID, 
        DateTime ReturnDate);
    public record AssignmentResponseDto(
        int AssignmentID, 
        int AssetID, 
        String AssetName,
        int AssignedToUserID,
        String AssignedToUserName,
        DateTime AssignedDate, 
        DateTime? ReturnDate, 
        AssignmentStatus Status, 
        string? Location);
}
