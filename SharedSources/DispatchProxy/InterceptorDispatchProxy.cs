namespace Microshaoft
{
    using System;
    using System.Reflection;

    public class InterceptorDispatchProxy<T> : DispatchProxy
    {
        public T Sender;
        public Func<T, MethodInfo, object[], bool> OnInvokingProcessFunc;
        public Action<T, MethodInfo, object[]> OnInvokedProcessAction;
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (OnInvokingProcessFunc != null)
            {
                var r = OnInvokingProcessFunc.Invoke(Sender, targetMethod, args);
                if
                    (
                        r
                        &&
                        //don't block intercept function
                        targetMethod.ReturnType == typeof(void)
                    )
                {
                    return null;
                }
            }
            try
            {
                return targetMethod.Invoke(Sender, args);
            }
            finally
            {
                OnInvokedProcessAction?.Invoke(Sender, targetMethod, args);
            }
        }
    }
}

namespace Test
{
    using Microshaoft;
    using System;
    using System.Reflection;
    internal class Program11122
    {
        static void Main(string[] args)
        {
            var proxy = DispatchProxy.Create<IProcessAble, InterceptorDispatchProxy<IProcessAble>>();
            InterceptorDispatchProxy<IProcessAble> commonDispatchProxy = (InterceptorDispatchProxy<IProcessAble>)proxy;

            commonDispatchProxy.Sender = new Processor();
            commonDispatchProxy.OnInvokingProcessFunc = (x, y, z) => { Console.WriteLine($"{nameof(commonDispatchProxy.OnInvokingProcessFunc)}:{y.Name}"); return true; };
            commonDispatchProxy.OnInvokedProcessAction = (x, y, z) => { Console.WriteLine($"{nameof(commonDispatchProxy.OnInvokedProcessAction)}:{y.Name}"); };

            proxy.Process();
            proxy.Process("asdasdas");
            Console.WriteLine();

            Console.ReadLine();

        }
    }

    public class Processor : IProcessAble
    {
        public void Process()
        {
            Console.WriteLine("Process");
        }

        public int Process(string x)
        {
            Console.WriteLine($"Process {x}");
            return 1;
        }
    }


    interface IProcessAble
    {
        void Process();
        int Process(string x);

    }
}

