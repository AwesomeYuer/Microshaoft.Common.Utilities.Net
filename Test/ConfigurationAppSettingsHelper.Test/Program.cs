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
            //Test Runtime Setting
            var appSettings = ConfigurationAppSettingsHelper
                                    .GetAppSettingsFormConfig<TestRuntimeSettings>();

            Console.ReadLine();
        }
    }
}
