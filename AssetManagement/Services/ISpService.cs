using System;
using System.Threading.Tasks;

namespace AssetManagement.Services
{
    public interface ISpService
    {
        Task AssignAssetAsync(int assetId, int assignedToUserId, DateTime assignedDate, string location);
        Task ReturnAssetAsync(int assignmentId, DateTime returnDate);
        Task LogIssueAsync(int assetId, int reportedByUserId, string description, bool requiresRepair);
        Task CompleteMaintenanceAsync(int maintenanceId, DateTime completedDate);
        Task GenerateReportAsync(string scope);
    }
}
