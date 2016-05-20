namespace Test
{
    using Microshaoft;
    using System;
    //using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    class Program
    {
        static void Main(string[] args)
        {
            var category = "Microshaoft-Test-Category-003"; 
            var instance = "Instance-003";
            //var enabledCountPerformance = false;
            CommonPerformanceCountersContainer container = null;
            EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                    .AttachPerformanceCountersCategoryInstance
                        (
                            category
                            , instance
                            , out container
                            , PerformanceCounterInstanceLifetime.Process
                            //, initializePerformanceCounterInstanceRawValue: 1009
                        );
            string input = string.Empty;
            while ("q" != (input = Console.ReadLine()))
            {
                Parallel
                        .For
                            (
                                0
                                , 5000
                                , new ParallelOptions() { MaxDegreeOfParallelism = 32 }
                                , (x) =>
                                {
                                    EasyPerformanceCountersHelper
                                            <CommonPerformanceCountersContainer>
                                                .TryCountPerformance
                                                    (
                                                        PerformanceCounterProcessingFlagsType
                                                                        .All
                                                        //.NonTimeBased
                                                        //.TimeBasedOnBeginOnEnd
                                                        //MultiPerformanceCountersTypeFlags.ProcessAllCounters
                                                        , category
                                                        , instance
                                                        , () => true
                                                        //, null
                                                        , () =>
                                                        {
                                                            Thread.Sleep(100);
                                                        //throw new Exception();
                                                        }
                                                        , (xx, yy) =>
                                                        {
                                                        //Console.WriteLine("on {0}, counterName {1} ", xx, yy.CounterName);
                                                        return 1;
                                                        }
                                                    );
                                }
                            );
            }
            Console.ReadLine();
        }
    }
}
