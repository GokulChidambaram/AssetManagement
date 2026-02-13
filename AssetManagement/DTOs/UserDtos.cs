using AssetManagement.Models.Enums;
using System.Text.Json.Serialization;

namespace AssetManagement.DTOs
{
    // UserCreateDto is not used; user creation is handled by AuthController.Register
    public record UserUpdateDto(
        string Name, 
        string Email, 
        int RoleID, 
        string? Department,
        [property : JsonConverter(typeof(JsonStringEnumConverter))]
        UserStatus Status);
    public record UserResponseDto(
        int UserID, 
        string Name, 
        string Email, 
        int RoleID, 
        String RoleName,
        string? Department,
        [property :JsonConverter(typeof(JsonStringEnumConverter))]
        UserStatus Status);

    public record UserRegisterDto(
        string Name, 
        string Email, 
        int RoleID, 
        string? Department, 
        string Password);
    public record UserLoginDto(
        string Email, 
        string Password);
}
