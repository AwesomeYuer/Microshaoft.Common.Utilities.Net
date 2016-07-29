namespace Test
{
    using Microshaoft;
    using System;
    //using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Collections.Generic;
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
       // public static DateTime? F0 = null;
    }
    class Program
    {
        static void Main(string[] args)
        {




            var entry = new XxxEntry()
            {
                F1 = "a"
                 ,
                F2 = DateTime.Now
                 ,
                F3 = null
                 ,
                F4 = 10
                 ,
                F5 = null
                 ,
                F6 = 10000
                 ,
                F7 = null
                 ,
                F8 = 0.1
                 ,
                F9 = 0.2
                 ,
                F10 = 0.3f
                 ,
                F11 = 0.4f
                 ,
                F12 = Guid.Empty
                 ,
                F13 = null
            };

            var list = new List<XxxEntry>();
            list.Add(entry);

            



             //var s1 = DynamicExpressionTreeHelper
             //    .CreateMemberGetter<XxxEntry, DateTime?>("F0")(entry);
             //var o = DynamicExpressionTreeHelper
             //    .CreateMemberGetter<XxxEntry, object>("F4")(entry);


             //Action<out object, object> action = (Action<object, object>) new Action<XxxEntry, object>((xx, yy) => { Console.WriteLine(xx); Console.WriteLine(yy); });

             //action(entry, "asdsad");

             var s = DynamicExpressionTreeHelper
                .CreateMemberGetter<XxxEntry, int>("F4")(entry);
            var o = DynamicExpressionTreeHelper
                .CreateMemberGetter<XxxEntry, object>("F4")(entry);

            //o1 报错
            //var o1 = DynamicExpressionTreeHelper
            //   .CreateMemberGetter<object, object>("F2")(entry);

            var o2 = DynamicExpressionTreeHelper
               .CreateMemberGetter(typeof(XxxEntry),"F2")(entry);

            var dataTable = list.ToDataTable();

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
                                                            Thread.Sleep(10);
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
