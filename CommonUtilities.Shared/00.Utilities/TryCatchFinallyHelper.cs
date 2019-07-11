
namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    public static class TryCatchFinallyProcessHelper
    {
        public static async Task<T> TryProcessCatchFinallyAsync<T>
                                    (
                                        bool needTry
                                        , Func<Task<T>> onTryProcessFunc
                                        , bool reThrowException = false
                                        , Func<Exception, bool> onCaughtExceptionProcessFunc = null
                                        , Action<bool, Exception> onFinallyProcessAction = null
                                    )
        {
            T r = default;
            //if (onTryProcessAction != null)
            {
                if (needTry)
                {
                    Exception exception = null;
                    var caughtException = false;
                    try
                    {
                        r = await onTryProcessFunc();
                        return r;
                    }
                    catch (Exception e)
                    {
                        caughtException = true;
                        exception = e;
                        var currentCalleeMethod = MethodBase.GetCurrentMethod();
                        var currentCalleeType = currentCalleeMethod.DeclaringType;
                        StackTrace stackTrace = new StackTrace();
                        StackFrame stackFrame = stackTrace.GetFrame(1);
                        var callerMethod = stackFrame.GetMethod();
                        var callerType = callerMethod.DeclaringType;
                        var frame = (stackTrace.FrameCount > 1 ? stackTrace.FrameCount - 1 : 1);
                        stackFrame = stackTrace.GetFrame(frame);
                        var originalCallerMethod = stackFrame.GetMethod();
                        var originalCallerType = originalCallerMethod.DeclaringType;
                        var innerExceptionMessage = string.Format
                                (
                                    "Rethrow caught [{1}] Exception{0} at Callee Method: [{2}]{0} at Caller Method: [{3}]{0} at Original Caller Method: [{4}]"
                                    , "\r\n\t"
                                    , e.Message
                                    , string.Format("{1}{0}{2}", "::", currentCalleeType, currentCalleeMethod)
                                    , string.Format("{1}{0}{2}", "::", callerType, callerMethod)
                                    , string.Format("{1}{0}{2}", "::", originalCallerType, originalCallerMethod)
                                );
                        Console.WriteLine(innerExceptionMessage);
                        if (onCaughtExceptionProcessFunc != null)
                        {
                            reThrowException = onCaughtExceptionProcessFunc(e);
                        }
                        if (reThrowException)
                        {
                            throw
                                new Exception
                                        (
                                            innerExceptionMessage
                                            , e
                                        );
                        }
                        return r;
                    }
                    finally
                    {
                        onFinallyProcessAction?.Invoke(caughtException, exception);
                    }
                }
                else
                {
                    return await onTryProcessFunc();
                }
            }
        }
        public static void TryProcessCatchFinally
                                    (
                                        bool needTry
                                        , Action onTryProcessAction
                                        , bool reThrowException = false
                                        , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                        , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                                    )
        {
            if (onTryProcessAction != null)
            {
                if (needTry)
                {
                    Exception exception = null;
                    Exception newException = null;
                    var caughtException = false;
                    var caughtInnerExceptionMessage = string.Empty;
                    try
                    {
                        onTryProcessAction();
                    }
                    catch (Exception e)
                    {
                        caughtException = true;
                        exception = e;
                        if (e is AggregateException aggregateException)
                        {
                            exception = aggregateException.Flatten();
                        }
                        var currentCalleeMethod = MethodBase.GetCurrentMethod();
                        var currentCalleeType = currentCalleeMethod.DeclaringType;
                        StackTrace stackTrace = new StackTrace(e, true);
                        StackFrame stackFrame = stackTrace.GetFrame(1);
                        var callerMethod = stackFrame.GetMethod();
                        var callerType = callerMethod.DeclaringType;
                        var frame = (stackTrace.FrameCount > 1 ? stackTrace.FrameCount - 1 : 1);
                        stackFrame = stackTrace.GetFrame(frame);
                        var originalCallerMethod = stackFrame.GetMethod();
                        var originalCallerType = originalCallerMethod.DeclaringType;
                        caughtInnerExceptionMessage = string.Format
                                (
                                    "Rethrow caught [{1}] Exception{0} at Callee Method: [{2}]{0} at Caller Method: [{3}]{0} at Original Caller Method: [{4}]"
                                    , "\r\n\t"
                                    , e.Message
                                    , string.Format("{1}{0}{2}", "::", currentCalleeType, currentCalleeMethod)
                                    , string.Format("{1}{0}{2}", "::", callerType, callerMethod)
                                    , string.Format("{1}{0}{2}", "::", originalCallerType, originalCallerMethod)
                                );
                        //Console.WriteLine(innerExceptionMessage);
                        if (onCaughtExceptionProcessFunc != null)
                        {
                            newException = new Exception
                                                (
                                                    caughtInnerExceptionMessage
                                                    , e
                                                );
                            reThrowException = onCaughtExceptionProcessFunc(e, newException, caughtInnerExceptionMessage);
                        }
                        if (reThrowException)
                        {
                            throw
                                newException;
                        }
                    }
                    finally
                    {
                        //Console.WriteLine("Finally");
                        onFinallyProcessAction?.Invoke(caughtException, exception, newException, caughtInnerExceptionMessage);
                    }
                }
                else
                {
                    onTryProcessAction();
                }
            }
        }

    }
}
