#if !NETFRAMEWORK4_6_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Threading.Tasks;

    public interface IStoreProcedureExecutable
    {
        string DataBaseType
        {
            get;
        }
        (
            bool Success
            , JToken Result
        )
            Execute
                (
                    string connectionString
                    , string storeProcedureName
                    , JToken parameters = null
                    , Func
                        <
                            IDataReader
                            , Type          // fieldType
                            , string        // fieldName
                            , int           // row index
                            , int           // column index
                            ,
                                (
                                    bool NeedDefaultProcess
                                    , JProperty Field   //  JObject Field 对象
                                )
                        > onReadRowColumnProcessFunc = null

                    , bool enableStatistics = false
                    , int commandTimeoutInSeconds = 90
                );
        Task
            <
                (
                    bool Success
                    , JToken Result
                )
            >
                ExecuteAsync
                    (
                        string connectionString
                        , string storeProcedureName
                        , JToken parameters = null
                        , Func
                            <
                                IDataReader
                                , Type          // fieldType
                                , string        // fieldName
                                , int           // row index
                                , int           // column index
                                ,
                                    (
                                        bool NeedDefaultProcess
                                        , JProperty Field   //  JObject Field 对象
                                    )
                            > onReadRowColumnProcessFunc = null
                        , bool enableStatistics = false
                        , int commandTimeoutInSeconds = 90
                    );
    }
    public interface IParametersDefinitionCacheAutoRefreshable
    {
        int CachedParametersDefinitionExpiredInSeconds
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
#endif