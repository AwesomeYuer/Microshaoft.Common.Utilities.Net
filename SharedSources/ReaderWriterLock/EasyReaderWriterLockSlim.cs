namespace Microshaoft
{
    using System;
    using System.Threading;
    public class EasyReaderWriterLockSlim
                        : ReaderWriterLockSlim
    {
        public EasyReaderWriterLockSlim()
            : base()
        {
        }
        public EasyReaderWriterLockSlim(LockRecursionPolicy recursionPolicy)
            : base(recursionPolicy)
        {
        }
        private int _isWriteLocked = 0;
        public bool IsWriteLocked
        {
            get
            {
                return (_isWriteLocked != 0);
            }
        }
        public void SafeRead
                        (
            //int enterReadLockTimeOutInSeconds
            //, 
                            Action<EasyReaderWriterLockSlim> onReadProcessAction
                        )
        {
            var r = false;
            try
            {
                if (_isWriteLocked == 1)
                {
                    //r = TryEnterReadLock(enterReadLockTimeOutInSeconds);
                    EnterReadLock();
                    r = true;
                }
                onReadProcessAction(this);
            }
            finally
            {
                if (r)
                {
                    //if (IsReadLockHeld)
                    {
                        ExitReadLock();
                    }
                }
            }
        }
        public void SafeWrite
                                (
            //int enterWriteLockTimeOutInSeconds
            //,
                                    Action<EasyReaderWriterLockSlim> onWriteProcessAction
                                )
        {
            var r = false;
            try
            {
                //r = TryEnterWriteLock(enterWriteLockTimeOutInSeconds);
                EnterWriteLock();
                r = true;
                if (r)
                {
                    Interlocked.Exchange(ref _isWriteLocked, 1);
                }
                //if (r)
                {
                    onWriteProcessAction(this);
                }
            }
            finally
            {
                if (r)
                {
                    //if
                    //    (
                    //        IsWriteLockHeld
                    //        &&
                    //        _isWriteLocked == 1
                    //    )
                    {
                        ExitWriteLock();
                        Interlocked.Exchange(ref _isWriteLocked, 0);
                    }
                }
            }
        }
    }
}

namespace ConsoleApplication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    //using Microshaoft;
    using Microshaoft;
    /// <summary>
    /// Class1 的摘要说明。
    /// </summary>
    public class Program1111
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        //[STAThread]
        static void Main116(string[] args)
        {
            string input = string.Empty;
            while ((input = Console.ReadLine()) != "q")
            {
                ParallelProcess();
            }
            Console.WriteLine("Hello World");
            Console.WriteLine(Environment.Version.ToString());
        }
        private static EasyReaderWriterLockSlim _easyReaderWriterLockSlim
                    = new EasyReaderWriterLockSlim();

        //private /*volatile*/ static int _isWriteLocked = 0;
        private static void ParallelProcess()
        {
            var stopWatch = Stopwatch.StartNew();
            Parallel.For
                (
                    0
                    , 1024 * 10
                    , new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 32
                    }
                    , (x) =>
                    {
                        DateTime? now = null;
                        //var r = false;
                        if (x == 10)
                        {
                            _easyReaderWriterLockSlim
                                .SafeWrite
                                    (
                                //10 * 1000
                                //, 
                                        (xx) =>
                                        {
                                            Console.WriteLine
                                                        (
                                                            "EnterWriteLock Current Thread [{0}], X [{1}], IsWriteLocked [{2}]"
                                                            , Thread.CurrentThread.ManagedThreadId
                                                            , x
                                                            , xx.IsWriteLocked
                                                        );
                                            Console.WriteLine("EnterWriteLock {0}", x);
                                            Console.WriteLine("Write {0}", x);
                                            Thread.Sleep(5 * 1000);
                                            now = DateTime.Now;
                                        }
                                    );
                            return;
                        }
                        else
                        {
                            _easyReaderWriterLockSlim
                                .SafeRead
                                    (
                                //10 * 1000
                                //,
                                        (xx) =>
                                        {
                                            if
                                                (
                                                    xx.IsReadLockHeld
                                                    ||
                                                    xx.IsWriteLocked
                                                )
                                            {
                                                Console.WriteLine
                                                    (
                                                        "read: {0}, IsWriteLocked {1}, IsWriteLockHeld {2}, IsReadLockHeld {3}"
                                                        , x
                                                        , xx.IsWriteLocked
                                                        , xx.IsWriteLockHeld
                                                        , xx.IsReadLockHeld
                                                    //, DateTime.Now
                                                    );
                                            }
                                        }
                                    );
                        }
                    }
                );
            stopWatch.Stop();
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
        }
    }
}
