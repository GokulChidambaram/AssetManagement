using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedLogAndMaintanence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===============================
            // sp_CompleteMaintenance (NEW)
            // ===============================
            migrationBuilder.Sql(@"
IF OBJECT_ID('sp_CompleteMaintenance', 'P') IS NOT NULL
   DROP PROCEDURE sp_CompleteMaintenance;
EXEC(N'
CREATE PROCEDURE sp_CompleteMaintenance
   @MaintenanceID INT,
   @CompletedDate DATETIME2
AS
BEGIN
   SET NOCOUNT ON;
   UPDATE t_maintenance
   SET
       Status = ''Completed'',
       CompletedDate = @CompletedDate,
       UpdatedAt = SYSUTCDATETIME()
   WHERE MaintenanceID = @MaintenanceID;
   DECLARE @AssetID INT =
       (SELECT AssetID FROM t_maintenance WHERE MaintenanceID = @MaintenanceID);
   IF @AssetID IS NOT NULL
   BEGIN
       UPDATE t_assets
       SET Status = ''Available'',
           UpdatedAt = SYSUTCDATETIME()
       WHERE AssetID = @AssetID;
   END
END
');
");
            // ===============================
            // sp_LogIssue (NEW)
            // ===============================
            migrationBuilder.Sql(@"
IF OBJECT_ID('sp_LogIssue', 'P') IS NOT NULL
   DROP PROCEDURE sp_LogIssue;
EXEC(N'
CREATE PROCEDURE sp_LogIssue
   @AssetID INT,
   @ReportedByUserID INT,
   @Description NVARCHAR(MAX),
   @RequiresRepair BIT
AS
BEGIN
   SET NOCOUNT ON;
   -- 1. CHECK CURRENT STATUS
   DECLARE @CurrentStatus NVARCHAR(50) =
       (SELECT Status FROM t_assets WHERE AssetID = @AssetID);
   IF @CurrentStatus IN (''UnderReview'', ''InRepair'')
   BEGIN
       RAISERROR(
           ''This asset is already under review or in repair. Duplicate issue blocked.'',
           16,
           1
       );
       RETURN;
   END
   -- 2. LOG THE ISSUE
   INSERT INTO t_issues
       (AssetID, ReportedByUserID, Description, ReportedDate, Status, CreatedAt)
   VALUES
       (@AssetID, @ReportedByUserID, @Description, SYSUTCDATETIME(), ''Open'', SYSUTCDATETIME());
   -- 3. UPDATE ASSET STATUS
   IF @RequiresRepair = 1
   BEGIN
       UPDATE t_assets
       SET Status = ''UnderReview'',
           UpdatedAt = SYSUTCDATETIME()
       WHERE AssetID = @AssetID;
   END
END
');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ===============================
            // sp_CompleteMaintenance (OLD)
            // ===============================
            migrationBuilder.Sql(@"
IF OBJECT_ID('sp_CompleteMaintenance', 'P') IS NOT NULL
   DROP PROCEDURE sp_CompleteMaintenance;
EXEC(N'
CREATE PROCEDURE sp_CompleteMaintenance
   @MaintenanceID INT,
   @CompletedDate DATETIME2
AS
BEGIN
   SET NOCOUNT ON;
   UPDATE t_maintenance
   SET Status = ''Completed'',
       CompletedDate = @CompletedDate,
       UpdatedAt = SYSUTCDATETIME()
   WHERE MaintenanceID = @MaintenanceID;
   DECLARE @AssetID INT =
       (SELECT AssetID FROM t_maintenance WHERE MaintenanceID = @MaintenanceID);
   IF @AssetID IS NOT NULL
   BEGIN
       UPDATE t_assets
       SET Status = ''Available'',
           UpdatedAt = SYSUTCDATETIME()
       WHERE AssetID = @AssetID;
   END
END
');
");
            // ===============================
            // sp_LogIssue (OLD)
            // ===============================
            migrationBuilder.Sql(@"
IF OBJECT_ID('sp_LogIssue', 'P') IS NOT NULL
   DROP PROCEDURE sp_LogIssue;
EXEC(N'
CREATE PROCEDURE sp_LogIssue
   @AssetID INT,
   @ReportedByUserID INT,
   @Description NVARCHAR(MAX),
   @RequiresRepair BIT
AS
BEGIN
   SET NOCOUNT ON;
   INSERT INTO t_issues
       (AssetID, ReportedByUserID, Description, ReportedDate, Status, CreatedAt)
   VALUES
       (@AssetID, @ReportedByUserID, @Description, SYSUTCDATETIME(), ''Open'', SYSUTCDATETIME());
   IF @RequiresRepair = 1
   BEGIN
       UPDATE t_assets
       SET Status = ''InRepair'',
           UpdatedAt = SYSUTCDATETIME()
       WHERE AssetID = @AssetID;
   END
END
');
");
        }
    }
}
