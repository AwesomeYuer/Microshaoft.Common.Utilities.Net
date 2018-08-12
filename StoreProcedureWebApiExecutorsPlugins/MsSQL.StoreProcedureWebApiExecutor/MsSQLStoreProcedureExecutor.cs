using System;
using System.Collections.Generic;
using System.Composition;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Microshaoft;
using Newtonsoft.Json.Linq;

namespace Microshaoft.StoreProcedureExecutors
{
    [Export(typeof(IStoreProcedureExecutable))]
    public class MsSQLStoreProcedureExecutor
                        : IStoreProcedureExecutable
                            , IStoreProcedureParametersSetCacheAutoRefreshable
    {
        public string DataBaseType => "mssql";////this.GetType().Name;

        public int CachedExecutingParametersExpiredInSeconds
        {
            get;
            set;
        }
        public bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get;
            set;
        }
        public bool Execute
                    (
                        string connectionString
                        , string storeProcedureName
                        , JToken parameters
                        , out JToken result
                    )
        {
            if
                (
                    CachedExecutingParametersExpiredInSeconds > 0
                    &&
                    MsSqlHelper
                        .CachedExecutingParametersExpiredInSeconds
                    !=
                    CachedExecutingParametersExpiredInSeconds
                )
            {
                MsSqlHelper
                        .CachedExecutingParametersExpiredInSeconds
                            = CachedExecutingParametersExpiredInSeconds;
            }
            result = null;
            SqlConnection connection = new SqlConnection(connectionString);
            result = MsSqlHelper
                            .StoreProcedureExecute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , 90
                                    );


            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                MsSqlHelper
                    .RefreshCachedStoreProcedureExecuted
                        (
                            connection
                            , storeProcedureName
                        );
            }
            return true;
        }
    }
}
