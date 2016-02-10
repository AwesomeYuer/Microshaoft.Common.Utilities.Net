namespace Microsoft.Boc
{
    using System;
    using System.Configuration;
    public static partial class SessionsHostServerHelper
    {
        public static string LocalSessionsHostServerMachineName
        {
            get
            { 
                var r = Environment.MachineName;
                var useEnvironmentVariableComputerNameAsMachineName = false;
                if
                    (
                        bool.TryParse
                            (
                                ConfigurationManager
                                    .AppSettings["UseEnvironmentVariableComputerNameAsMachineName"]
                                , out useEnvironmentVariableComputerNameAsMachineName
                            )
                    )
                {
                    if (useEnvironmentVariableComputerNameAsMachineName)
                    {
                        r = Environment
                                .GetEnvironmentVariable
                                    (
                                        "COMPUTERNAME"
                                        , EnvironmentVariableTarget.Process
                                    );
                    }
                }
                return r;
            }
        }
    }
}
