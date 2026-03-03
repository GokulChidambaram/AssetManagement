using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagement.Migrations
{
    /// <inheritdoc />
    public partial class createbyandupdatedby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_roles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_roles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_maintenance",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_maintenance",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_issues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_issues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_assignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_assignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "t_assets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "t_assets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_roles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_roles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_reports");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_reports");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_maintenance");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_maintenance");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_issues");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_issues");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_categories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_categories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_assignments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_assignments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "t_assets");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "t_assets");
        }
    }
}
