namespace AssetManagement.DTOs
{
    public record CategoryCreateDto(
        string Name);
    public record CategoryUpdateDto(
        string Name);
    public record CategoryResponseDto(
        int CategoryID, 
        string Name);
}
