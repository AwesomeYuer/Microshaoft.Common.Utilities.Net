namespace Microshaoft
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    public static class WindowTaskProcessorHelper
    {
        public static async Task WindowUITaskRunAsync
                           (
                               Func<Window> onCreateProgressWindowProcessFunc
                               , Action<Window> taskRunWindowProcessAction
                               , Window ownerWindow
                           )

        {
            var autoResetEvent = new AutoResetEvent(false);
            Window progressWindow = null;
            var thread = new Thread
                                (
                                    () =>
                                    {
                                        Thread.Sleep(10);
                                        progressWindow = onCreateProgressWindowProcessFunc();
                                        autoResetEvent.Set();
                                        progressWindow.Show();
                                        System.Windows.Threading.Dispatcher.Run();
                                    }
                                );
            thread.TrySetApartmentState(ApartmentState.STA);
            thread.Start();
            autoResetEvent.WaitOne();
            await Task
                    .Run
                        (
                            () =>
                            {
                                taskRunWindowProcessAction(ownerWindow);
                                progressWindow
                                    .Dispatcher
                                    .Invoke
                                        (
                                            () =>
                                            {
                                                progressWindow.Close();
                                            }
                                        );
                            }
                        );
        }
    }
}
