﻿namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    public interface IStoreProcedureExecutable
    {
        string DataBaseType { get; }
        bool Execute
                (
                    string connectionString
                    , string storeProcedureName
                    , JToken parameters
                    , out JToken result
                    , int commandTimeoutInSeconds = 90
                );


    }

    public interface IStoreProcedureParametersSetCacheAutoRefreshable
    {
        int CachedExecutingParametersExpiredInSeconds
        {
            get;
            set;
        }

        bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get;
            set;
        }

    }
}
