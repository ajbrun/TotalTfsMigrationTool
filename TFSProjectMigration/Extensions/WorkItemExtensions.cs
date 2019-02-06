using log4net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;

namespace TFSProjectMigration.Extensions
{
    public static class WorkItemExtensions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public static void RetrySave(this WorkItem workItem, int retryTimes = 100)
        {
            int i = 0;
            while (i < retryTimes)
            {
                try
                {
                    workItem.Save();
                    return;
                }
                catch (Exception e)
                {
                    logger.Error($"Work item not <{workItem.Title}> saved", e);
                }
                finally
                {
                    i++;
                }
            }
        }
    }
}
