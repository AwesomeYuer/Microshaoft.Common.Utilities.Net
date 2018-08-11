using System;
using System.Collections.Generic;
using System.Composition;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Microshaoft;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Microshaoft.StoreProcedureExecutors
{
    [Export(typeof(IStoreProcedureExecutable))]
    public class MySQLStoreProcedureExecutor
                        : IStoreProcedureExecutable
                            , IStoreProcedureParametersSetCacheAutoRefreshable
    {
        public string DataBaseType => "mysql";////this.GetType().Name;

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
                    MySqlHelper
                        .CachedExecutingParametersExpiredInSeconds
                    !=
                    CachedExecutingParametersExpiredInSeconds
                )
            {
                MySqlHelper
                        .CachedExecutingParametersExpiredInSeconds
                            = CachedExecutingParametersExpiredInSeconds;
            }
            result = null;
            MySqlConnection connection = new MySqlConnection(connectionString);
            result = MySqlHelper
                            .StoreProcedureExecute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , 90
                                    );


            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                MySqlHelper
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
