using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AssetManagement.Data;

namespace AssetManagement.Services
{
    public class SpService : ISpService
    {
        private readonly ApplicationDbContext _db;

        public SpService(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task AssignAssetAsync(int assetId, int assignedToUserId, DateTime assignedDate, string location)
        {
            var p = new[]
            {
                new SqlParameter("@AssetID", assetId),
                new SqlParameter("@AssignedToUserID", assignedToUserId),
                new SqlParameter("@AssignedDate", assignedDate),
                new SqlParameter("@Location", (object?)location ?? DBNull.Value)
            };

            return _db.Database.ExecuteSqlRawAsync("EXEC sp_AssignAsset @AssetID, @AssignedToUserID, @AssignedDate, @Location", p);
        }

        public Task ReturnAssetAsync(int assignmentId, DateTime returnDate)
        {
            var p = new[]
            {
                new SqlParameter("@AssignmentID", assignmentId),
                new SqlParameter("@ReturnDate", returnDate)
            };

            return _db.Database.ExecuteSqlRawAsync("EXEC sp_ReturnAsset @AssignmentID, @ReturnDate", p);
        }

        public Task LogIssueAsync(int assetId, int reportedByUserId, string description, bool requiresRepair)
        {
            var p = new[]
            {
                new SqlParameter("@AssetID", assetId),
                new SqlParameter("@ReportedByUserID", reportedByUserId),
                new SqlParameter("@Description", (object?)description ?? DBNull.Value),
                new SqlParameter("@RequiresRepair", requiresRepair)
            };

            return _db.Database.ExecuteSqlRawAsync("EXEC sp_LogIssue @AssetID, @ReportedByUserID, @Description, @RequiresRepair", p);
        }

        public Task CompleteMaintenanceAsync(int maintenanceId, DateTime completedDate)
        {
            var p = new[]
            {
                new SqlParameter("@MaintenanceID", maintenanceId),
                new SqlParameter("@CompletedDate", completedDate)
            };

            return _db.Database.ExecuteSqlRawAsync("EXEC sp_CompleteMaintenance @MaintenanceID, @CompletedDate", p);
        }

        public Task GenerateReportAsync(string scope)
        {
            var p = new[]
            {
                new SqlParameter("@Scope", (object?)scope ?? DBNull.Value)
            };

            return _db.Database.ExecuteSqlRawAsync("EXEC sp_GenerateReport @Scope", p);
        }
    }
}
