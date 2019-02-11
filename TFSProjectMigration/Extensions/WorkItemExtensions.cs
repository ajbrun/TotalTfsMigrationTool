using log4net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSProjectMigration.Extensions
{
    public static class WorkItemExtensions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public static void RetrySave(this WorkItem workItem, int maxAttempts = 100)
        {
            ConnectionHelper.Retry(() => workItem.Save(), $"SaveWorkItem: {workItem.Type.Name} {workItem.State} {workItem.Title}", maxAttempts);
        }
    }
}
