
namespace Microshaoft
{
    using System;
    using System.Threading;
    public static class GCNotifier
    {
        public static void CancelForFullGCNotification()
        {
            GC.CancelFullGCNotification();
        }
        public static void RegisterForFullGCNotification
                            (
                                int maxGenerationThreshold
                                , int maxLargeObjectHeapThreshold
                                , int waitOnceSecondsTimeout
                                , Action<GCNotificationStatus> waitForFullGCApproachProcessAction
                                , Action<GCNotificationStatus> waitForFullGCCompleteProcessAction
                            )
        {
            GC.RegisterForFullGCNotification(maxGenerationThreshold, maxLargeObjectHeapThreshold);
            new Thread
                (
                    new ThreadStart
                        (
                            () =>
                            {
                                while (true)
                                {
                                    if (waitForFullGCApproachProcessAction != null)
                                    {
                                        var gcNotificationStatus
                                                = GC.WaitForFullGCApproach(1000 * waitOnceSecondsTimeout);
                                        if (gcNotificationStatus != GCNotificationStatus.Timeout)
                                        {
                                            waitForFullGCApproachProcessAction(gcNotificationStatus);
                                        }
                                    }
                                    if (waitForFullGCApproachProcessAction != null)
                                    {
                                        var gcNotificationStatus
                                                = GC.WaitForFullGCComplete(1000 * waitOnceSecondsTimeout);
                                        if (gcNotificationStatus != GCNotificationStatus.Timeout)
                                        {
                                            waitForFullGCCompleteProcessAction(gcNotificationStatus);
                                        }
                                    }
                                    Thread.Sleep(1000);
                                }
                            }
                        )
                    ).Start();
        }
    }
}
