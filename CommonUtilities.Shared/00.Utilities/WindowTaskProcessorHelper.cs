#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    public class Program
    {
        static void Main(string[] args)
        {
            /*
             * thanks
             * https://walterlv.com/post/convert-async-to-sync-by-push-frame.html
            */
            Console.Title = "walterlv's demo";
            var foo = Foo();
            var result = WindowUITaskProcessorHelper.AwaitByPushFrame(foo);
            Console.WriteLine($"输入的字符串为：{result}");
            Console.ReadKey();
        }

        private static async Task<string> Foo()
        {
            Console.WriteLine("请稍后……");
            await Task.Delay(1000);
            Console.Write("请输入：");
            var line = Console.ReadLine();
            Console.WriteLine("正在处理……");
            await Task.Run(() =>
            {
                // 模拟耗时的操作。
                Thread.Sleep(1000);
            });
            return line;
        }
    }
    
    public static class WindowUITaskProcessorHelper
    {
        /// <summary>
        /// 通过 PushFrame（进入一个新的消息循环）的方式来同步等待一个必须使用 await 才能等待的异步操作。
        /// 由于使用了消息循环，所以并不会阻塞 UI 线程。<para/>
        /// 此方法适用于将一个 async/await 模式的异步代码转换为同步代码。<para/>
        /// </summary>
        /// <remarks>
        /// 此方法适用于任何线程，包括 UI 线程、非 UI 线程、STA 线程、MTA 线程。
        /// </remarks>
        /// <typeparam name="TResult">
        /// 异步方法返回值的类型。
        /// 我们认为只有包含返回值的方法才会出现无法从异步转为同步的问题，所以必须要求异步方法返回一个值。
        /// </typeparam>
        /// <param name="task">异步的带有返回值的任务。</param>
        /// <returns>异步方法在同步返回过程中的返回值。</returns>
        public static TResult AwaitByPushFrame<TResult>(this Task<TResult> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            Contract.EndContractBlock();

            var frame = new DispatcherFrame();
            task.ContinueWith(t =>
            {
                frame.Continue = false;
            });
            Dispatcher.PushFrame(frame);
            return task.Result;
        }



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
#endif