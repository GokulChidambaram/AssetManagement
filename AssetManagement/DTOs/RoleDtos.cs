namespace AssetManagement.DTOs
{
    public record RoleCreateDto(string Name);
    public record RoleUpdateDto(string Name);
    public record RoleResponseDto(int RoleID, string Name);
}
