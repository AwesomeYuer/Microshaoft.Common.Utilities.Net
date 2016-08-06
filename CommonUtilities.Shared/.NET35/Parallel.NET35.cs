#if NET35
namespace System.Threading.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;
    using System.Text;

    public static class Parallel
    {
        public static void For
                        (
                            int formInclusive
                            , int toExclusive
                            , ParallelOptions parallelOptions
                            , Action<int> body
                        )
        {
            for (int i = formInclusive; i < toExclusive; i++)
            {
                //ThreadPool.QueueUserWorkItem
                //            (
                //                new WaitCallback
                //                    (
                //                        (x) =>
                //                        {
                                            body(i);
                            //            }
                            //        )
                            //);   
            }
        }
    }
    public class ParallelOptions
    {
        public int MaxDegreeOfParallelism = Environment.ProcessorCount; 
    }
    
}
#endif
