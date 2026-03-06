using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagement.Migrations
{
    /// <inheritdoc />
    public partial class sp_approveandassignasset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🚀 Create the Stored Procedure
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_ApproveAndAssignAsset
                    @RequestID INT,
                    @AssetID INT,
                    @AssignedToUserID INT,
                    @Location NVARCHAR(200)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    BEGIN TRANSACTION;

                    BEGIN TRY
                        -- 1. Check if Request exists and is still Pending (Status 0)
                        IF NOT EXISTS (SELECT 1 FROM AssetRequests WHERE RequestID = @RequestID AND Status = 0)
                        BEGIN
                            RAISERROR('Request is not in Pending status.', 16, 1);
                            ROLLBACK TRANSACTION;
                            RETURN;
                        END

                        -- 2. Validate Asset is Available
                        IF NOT EXISTS (SELECT 1 FROM t_assets WHERE AssetID = @AssetID AND Status = 'Available')
                        BEGIN
                            RAISERROR('Asset is not available for assignment.', 16, 1);
                            ROLLBACK TRANSACTION;
                            RETURN;
                        END

                        -- 3. Create the Assignment
                        INSERT INTO t_assignments (AssetID, AssignedToUserID, AssignedDate, Status, Location, CreatedAt)
                        VALUES (@AssetID, @AssignedToUserID, SYSUTCDATETIME(), 'Active', @Location, SYSUTCDATETIME());

                        -- 4. Update Asset Status
                        UPDATE t_assets SET Status = 'Assigned', UpdatedAt = SYSUTCDATETIME() WHERE AssetID = @AssetID;

                        -- 5. Update Request Status to Approved (Status 1)
                        UPDATE AssetRequests SET Status = 1, UpdatedAt = SYSUTCDATETIME() WHERE RequestID = @RequestID;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        ROLLBACK TRANSACTION;
                        THROW;
                    END CATCH
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 🗑️ Remove the procedure if the migration is rolled back
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ApproveAndAssignAsset");
        }
    }
}
