﻿using log4net;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;

namespace TFSProjectMigration.Extensions
{
    public static class WorkItemExtensions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public static void RetrySave(this WorkItem workItem, int maxAttempts = 100)
        {
            ConnectionHelper.Retry(() => workItem.Save(), $"SaveWorkItem: {workItem.Type.Name} {workItem.State} {workItem.Title}", maxAttempts);

            //int i = 0;
            //while (i < retryTimes)
            //{
            //    try
            //    {
            //        workItem.Save();
            //        return;
            //    }
            //    catch (TeamFoundationServiceUnavailableException e)
            //    {
            //        logger.Error($"Work item not <{workItem.Title}> saved. Retrying...", e);
            //    }
            //    finally
            //    {
            //        i++;
            //    }
            //}
        }
    }
}
