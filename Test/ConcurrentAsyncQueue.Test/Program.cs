namespace Test
{
    using Microshaoft;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Linq;
    using System.Data;
    class Program
    {

        public class XxxEntry
        {
            public string F1;
            public DateTime F2;
            public DateTime? F3;
            public int F4;
            public int? F5;
            public long F6;
            public long? F7;
            public double F8;
            public double? F9;
            public float F10;
            public float? F11;
            public Guid F12;
            public Guid? F13;
            public static DateTime? F0 = null;
        }

        static void Main(string[] args)
        {


            var enabledCountPerformance = true;
            var singleThreadAsyncDequeueProcessor = new SingleThreadAsyncDequeueProcessor<XxxEntry>();
            singleThreadAsyncDequeueProcessor
                .AttachPerformanceCountersCategoryInstance
                    (
                        "Microshaoft-SingleThreadAsyncDequeueProcessor"
                        , "new"
                        , () =>
                        {
                            return enabledCountPerformance;
                        }
                    );
            //singleThreadAsyncDequeueProcessor.OnCaughtException
            Console.WriteLine("SingleThreadAsyncDequeueProcessor testing .... press any key!");
            Console.ReadLine();
            singleThreadAsyncDequeueProcessor
                .StartRunDequeuesThreadProcess
                    (
                        (x, y) =>
                        {

                        }
                        , 1000
                        , (x, y) =>
                        {
                            var list = y.Select
                                            (
                                                (xx) =>
                                                {
                                                    return xx.Item2;
                                                }
                                            );
                            var dataTable = list.ToDataTable();
                            Console.WriteLine(dataTable.Rows.Count);

                        }
                        ,1000
                        , 100
                        , (x, y, z) =>
                        {
                            return false;
                        }
                    );

            Parallel
                .For
                    (
                        1
                        , 10
                        , new ParallelOptions() { MaxDegreeOfParallelism = 1  }
                        , (x) =>
                        {
                            var entry = new XxxEntry()
                            {
                                F1 = "a"
                                , F2 = DateTime.Now
                                , F3 = DateTime.Now.AddDays(10)
                                , F4 = 10
                                , F5 = null
                                , F6 = 10000
                                , F7 = null
                                , F8 = 0.1
                                , F9 = 0.2
                                , F10 = 0.3f
                                , F11 = 0.4f
                                , F12 = Guid.NewGuid()
                                , F13 = Guid.Empty
                            };
                            singleThreadAsyncDequeueProcessor
                                .Enqueue(entry);
                        }
                    );



            






            Console.WriteLine("ConcurrentAsyncQueue testing .... press any key!");
            Console.ReadLine();
            
            var q = new ConcurrentAsyncQueue<int>();
            q.AttachPerformanceCounters
                    (
                        "Microshaoft ConcurrentAsyncQueue Performance Counters"
                        , "new"
                        , new QueuePerformanceCountersContainer()
                        , () =>
                        {
                            return enabledCountPerformance;
                        }
                        , PerformanceCounterInstanceLifetime.Process
                        , 0
                    );
            Random random = new Random();
            q.OnDequeue += //new ConcurrentAsyncQueue<int>.QueueEventHandler
                            (
                                (x) =>
                                {
                                    //int sleep = random.Next(0, 9) * 5;
                                    //Console.WriteLine(sleep);
                                    Thread.Sleep(50);
                                    //if (sleep > 400)
                                    //{
                                    //    throw new Exception();
                                    //    Console.WriteLine(x);
                                    //}
                                   // Console.WriteLine("ThreadID:{0}", Thread.CurrentThread.ManagedThreadId);

                                }
                            );
            q.OnEnqueueProcessCaughtException += //new ConcurrentAsyncQueue<int>.ExceptionEventHandler
                                    (
                                        (x, y, z, w) =>
                                        {
                                            Console.WriteLine(y.ToString());
                                            return false;
                                        }
                                    );
            q.OnDequeueProcessCaughtException += //new ConcurrentAsyncQueue<int>.ExceptionEventHandler
                                                (
                                                    (x, y, z, w) =>
                                                    {
                                                        Console.WriteLine(y.ToString());
                                                        return false;
                                                    }
                                                );

            Console.WriteLine("begin ...");
            //q.StartAdd(10);
            string r = string.Empty;
            while ((r = Console.ReadLine()) != "q")
            {
                int i;
                if (int.TryParse(r, out i))
                {
                    Console.WriteLine("Parallel Enqueue {0} begin ...", i);
                    new Thread
                            (
                                new ParameterizedThreadStart
                                            (
                                                (x) =>
                                                {
                                                    Parallel.For
                                                                (
                                                                    0
                                                                    , i
                                                                    , (xx) =>
                                                                    {
                                                                        q.Enqueue(xx);
                                                                    }
                                                                );
                                                    Console.WriteLine("Parallel Enqueue {0} end ...", i);
                                                }
                                            )
                            ).Start();
                }
                else if (r.ToLower() == "stop")
                {
                    q.StartDecreaseDequeueProcessThreads(10);
                }
                else if (r.ToLower() == "add")
                {
                    q.StartIncreaseDequeueProcessThreads(20);
                }
                else if (r.ToLower() == "countstart")
                {
                    enabledCountPerformance = true;
                }
                else if (r.ToLower() == "countstop")
                {
                    enabledCountPerformance = false;
                }
                else if (r.ToLower() == "clear")
                {
                    q.ClearPerformanceCountersValues(10);
                    q.ClearPerformanceCountersValues(100);
                    q.ClearPerformanceCountersValues(1000);
                }
                else if (r.ToLower() == "pool")
                {
                    var s = string.Format
                                    (
                                        "Pool.Count: [{0}], PooledObjectsCount Got:[{1}]-Return:[{2}], NonPooledObjectsCount Got:[{3}]-Release:[{4}]"
                                       , q
                                            .StopwatchsPool
                                            .Pool
                                            .Count
                                       , q
                                            .StopwatchsPool
                                            .PooledObjectsGotCount
                                       , q
                                            .StopwatchsPool
                                            .PooledObjectsReturnCount
                                       , q
                                            .StopwatchsPool
                                            .NonPooledObjectsGotCount
                                       , q
                                            .StopwatchsPool
                                            .NonPooledObjectsReleaseCount
                                    );
                    Console.WriteLine(s);
                    
                }
                else
                {
                    Console.WriteLine("please input Number!");
                }
            }
            Console.ReadLine();
        }
    }
}
