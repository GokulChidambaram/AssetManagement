namespace AssetManagement.DTOs
{
    // What the Employee sends to the API (CategoryID and the reason why they need it)
    public record AssetRequestCreateDto(int CategoryID, string Reason);

    // What the API sends back to the Angular Dashboard (Includes the Category Name and Status)
    public record AssetRequestResponseDto(
        int RequestID,
        string CategoryName,
        string Reason,
        string Status,
        DateTime CreatedAt
    );
}
