namespace AssetManagement.DTOs
{
    public record ReportCreateDto(string? Scope);
    public record ReportResponseDto(
        int ReportID, 
        string? Scope, 
        string? Metrics, 
        DateTime GeneratedDate);
}
