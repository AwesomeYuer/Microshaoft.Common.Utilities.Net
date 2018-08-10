//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Text;
//using Microshaoft;
//using Newtonsoft.Json.Linq;

//namespace Microshaoft.StoreProcedureExecutors
//{
//    public class MsSQLStoreProcedureExecutor 
//                        : IStoreProcedureExecutable
//                            , IStoreProcedureParametersSetCacheAutoRefreshable
//    {
//        public string DataBaseType => this.GetType().Name;

//        public int CachedExecutingParametersExpiredInSeconds
//        {
//            get;
//            set;
//        }
//        public bool NeedAutoRefreshExecutedTimeForSlideExpire
//        {
//            get;
//            set;
//        }
//        public bool Execute
//                    (
//                        string connectionString
//                        , string storeProcedureName
//                        , JToken parameters
//                        , out JToken result
//                    )
//        {
//            result = null;
//            SqlConnection connection = new SqlConnection(connectionString);
//            result = SqlHelper
//                            .StoreProcedureExecute
//                                    (
//                                        connection
//                                        , storeProcedureName
//                                        , parameters
//                                        , 90
//                                    );
//            if (NeedAutoRefreshExecutedTimeForSlideExpire)
//            {
//                SqlHelper
//                    .RefreshCachedStoreProcedureExecuted
//                        (
//                            connection
//                            , storeProcedureName
//                        );
//            }
//            return true;
//        }
//    }
//}
