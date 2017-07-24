namespace Microshaoft
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    public static class WindowTaskProcessorHelper
    {
        public static async Task WindowUITaskRunAsync
                           (
                               Func<Window> onCreateProgressWindowProcessFunc
                               , Action<Window> onWindowUITaskProcessAction
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
                                        Dispatcher.Run();
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
                                onWindowUITaskProcessAction(ownerWindow);
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
