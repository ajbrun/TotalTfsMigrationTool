using log4net;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Threading;

namespace TFSProjectMigration
{
    public static class ConnectionHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public static T Retry<T>(Func<T> func, string contextMessage, int maxAttempts = 100)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    return func();
                }
                catch (TeamFoundationServiceUnavailableException e)
                {
                    HandleException(i, maxAttempts, contextMessage, e);
                    DelayMs(i);
                }
            }

            logger.Error("Retries failed.");
            return default(T);
        }

        public static void Retry(Action func, string contextMessage, int maxAttempts = 100)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    func();
                    return;
                }
                catch (TeamFoundationServiceUnavailableException e)
                {
                    HandleException(i, maxAttempts, contextMessage, e);
                    DelayMs(i);
                }
                catch (FileAttachmentException e)
                {
                    HandleException(i, maxAttempts, contextMessage, e);
                    DelayMs(i);
                }
            }

            logger.Error("Retries failed.");
        }

        private static void DelayMs(int attemptIndex, int msDelay = 10)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds((attemptIndex + 1) * msDelay));
        }

        private static void HandleException(int iterationIndex, int maxAttempts, string contextMessage, Exception e)
        {
            logger.Error(contextMessage);
            logger.Error($"Failed {iterationIndex + 1} out of {maxAttempts} times.", e);
        }
    }
}
