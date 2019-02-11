using log4net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSProjectMigration.Extensions
{
    public static class WorkItemStoreExtensions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public static WorkItem RetryGetWorkItem(this WorkItemStore workItemStore, int workItemId, int maxAttempts = 100)
        {
            return ConnectionHelper.Retry(() => workItemStore.GetWorkItem(workItemId), $"GetWorkItem: {workItemId}", maxAttempts);
        }
    }
}
