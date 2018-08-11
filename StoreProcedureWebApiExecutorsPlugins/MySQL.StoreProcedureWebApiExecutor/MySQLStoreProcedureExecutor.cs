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
    public class MySQLStoreProcedureExecutor
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
                    SqlHelper
                        .CachedExecutingParametersExpiredInSeconds
                    !=
                    CachedExecutingParametersExpiredInSeconds
                )
            {
                SqlHelper
                        .CachedExecutingParametersExpiredInSeconds
                            = CachedExecutingParametersExpiredInSeconds;
            }
            result = null;
            DbConnection connection = new SqlConnection(connectionString);
            result = SqlHelper
                            .StoreProcedureExecute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , 90
                                    );


            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                SqlHelper
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
