using log4net;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;

namespace TFSProjectMigration.Extensions
{
    public static class WorkItemStoreExtensions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public static WorkItem RetryGetWorkItem(this WorkItemStore workItemStore, int workItemId, int maxAttempts = 100)
        {
            return ConnectionHelper.Retry(() => workItemStore.GetWorkItem(workItemId), $"GetWorkItem: {workItemId}", maxAttempts);

            //int i = 0;
            //while (i < retryTimes)
            //{
            //    try
            //    {
            //        return workItemStore.GetWorkItem(workItemId);
            //    }
            //    catch (TeamFoundationServiceUnavailableException e)
            //    {
            //        logger.Error($"Work item ID <{workItemStore}> could not be fetched. Retrying...", e);
            //    }
            //    finally
            //    {
            //        i++;
            //    }
            //}

            //return null;
        }
    }
}
