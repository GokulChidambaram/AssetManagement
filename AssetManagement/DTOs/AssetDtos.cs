
using AssetManagement.Models.Enums;
using System.Text.Json.Serialization;

namespace AssetManagement.DTOs

{

    public record AssetCreateDto(
        string Name,
        string Description,
        string ModelNo,
        string DepartmentName,
        string SupplierName,
        int CategoryID, string? Tag, DateTime? PurchaseDate, decimal? Cost);

	//public record AssetUpdateDto(string Name, int CategoryID, string? Tag, DateTime? PurchaseDate, decimal? Cost, AssetStatus Status);
	public record AssetUpdateDto(
		string Name,
		string CategoryName, // Changed from CategoryID to CategoryName
		string ModelNo,      // Added ModelNo
		string? Tag,
		DateTime? PurchaseDate,
		decimal? Cost,
		AssetStatus Status);
	public record AssetResponseDto(int AssetID, 
        string Name, 
        int CategoryID,
        string CategoryName, 
        string? Tag, 
        DateTime? PurchaseDate,
		string ModelNo,
		decimal? Cost,
        [property : JsonConverter(typeof(JsonStringEnumConverter))]
        AssetStatus Status, 
        DateTime CreatedAt, 
        DateTime? UpdatedAt);

}

