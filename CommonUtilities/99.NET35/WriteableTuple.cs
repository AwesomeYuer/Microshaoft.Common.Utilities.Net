namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    public class WriteableTuple
    {
        public static WriteableTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new WriteableTuple<T1, T2>(item1, item2);
        }
        public static WriteableTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new WriteableTuple<T1, T2, T3>(item1, item2, item3);
        }
        public static WriteableTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new WriteableTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
        public static WriteableTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new WriteableTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }
    public class WriteableTuple<T1, T2>
    {
        public WriteableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
        public T1 Item1;
        public T2 Item2;
    }
    public class WriteableTuple<T1, T2, T3>
    {
        public WriteableTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
    }
    public class WriteableTuple<T1, T2, T3, T4>
    {
        public WriteableTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
    }
    public class WriteableTuple<T1, T2, T3, T4, T5>
    {
        public WriteableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
    }
}
