using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microshaoft
{
    public interface IStoreProcedureExecutable
    {
        string DataBaseType { get; }
        bool Execute
                (
                    string connectionString
                    , string storeProcedureName
                    , JToken parameters
                    , out JToken result
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
