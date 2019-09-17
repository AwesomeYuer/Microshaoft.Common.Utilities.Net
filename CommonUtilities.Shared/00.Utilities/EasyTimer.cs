
namespace Microshaoft
{
    using System;
//#if NETFRAMEWORK4_X
    using System.Timers;
//#else
//    using System.Threading;
//#endif
    public class EasyTimer
    {
        private Timer _timer;
        public void Start()
        {
            if (_timer != null)
            {
                _timer.Start();
            }
        }
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }
        private int _intervalInSeconds;

        public int IntervalInSeconds
        {
            get { return _intervalInSeconds; }
        }
        public void SetIntervalInSeconds(int seconds)
        {
            _intervalInSeconds = seconds;
            _timer.Interval = seconds * 1000;
        }
        public EasyTimer
                    (
                        int intervalInSeconds
                        , int times         //action 耗时倍数
                        , Action<EasyTimer> timerProcessAction = null
                        , bool autoStart = true
                        , bool skipFirstTimerProcessAction = true
                        , Func<EasyTimer, Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null

                    )
        {
            if (timerProcessAction == null)
            {
                return;
            }
            _intervalInSeconds = intervalInSeconds;
            //2015-01-08 解决第一次 Null
            _timer = new Timer(_intervalInSeconds * 1000);
            //first 主线程
            if (!skipFirstTimerProcessAction)
            {
                TimerProcessAction(times, timerProcessAction, onCaughtExceptionProcessFunc);
            }
            
            _timer.Elapsed += new ElapsedEventHandler
                                        (
                                            (x, y) =>
                                            {
                                                TimerProcessAction
                                                    (
                                                        times
                                                        , timerProcessAction
                                                        , onCaughtExceptionProcessFunc
                                                    );
                                            }
                                        );
            if (autoStart)
            {
                Start();
            }
        }
        private object _locker = new object();
        private void TimerProcessAction
                        (
                            int times
                            , Action<EasyTimer> timerAction
                            , Func<EasyTimer, Exception, Exception, string, bool> onCaughtExceptionProcessFunc
                        )
        {

            try
            {
                if (timerAction == null)
                {
                    return;
                }
                if (_timer != null)
                {
                    lock (_locker)
                    {
                        _timer.Stop();
                        _timer.Enabled = false;
                    }
                }
                DateTime begin;
                do
                {
                    begin = DateTime.Now;
                    TryCatchFinallyProcessHelper
                        .TryProcessCatchFinally
                            (
                                true
                                , () =>
                                {
                                    timerAction(this);
                                }
                                , false
                                , (x, y, w) =>
                                {
                                    var reThrowException = false;
                                    if (onCaughtExceptionProcessFunc != null)
                                    {
                                        reThrowException = onCaughtExceptionProcessFunc(this, x, y, w);
                                    }
                                    return reThrowException;
                                }
                                , null
                            );
                } while (Math.Abs(DateTimeHelper.SecondsDiffNow(begin)) > times * _intervalInSeconds);
            }
            finally
            {
                if (_timer != null)
                {
                    lock (_locker)
                    {
                        _timer.Enabled = true;
                        _timer.Start();
                    }
                }
            }
            
        }
    }
}