//#if !NETFRAMEWORK4_6_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Threading.Tasks;

    public partial interface IStoreProcedureExecutable
    {
        string DataBaseType
        {
            get;
        }

        (
            bool Success
            , JToken Result
        )
            ExecuteJsonResults
                (
                    string connectionString
                    , string storeProcedureName
                    , JToken parameters = null
                    , Func
                        <
                            int             // resultSet index
                            , IDataReader
                            , int           // row index
                            , int           // column index
                            , Type          // fieldType
                            , string        // fieldName
                            ,
                                (
                                    bool NeedDefaultProcess
                                    , JProperty Field   //  JObject Field 对象
                                )
                        > onReadRowColumnProcessFunc = null

                    , bool enableStatistics = false
                    , int commandTimeoutInSeconds = 90
                );

        void
            ExecuteReaderProcess
                (
                    string connectionString
                    , string storeProcedureName
                    , JToken parameters = null
                    , Action
                        <
                            int             // resultset Index
                            , JArray        // columns
                            , IDataReader
                            , int           // row index
                        > onReadRowProcessAction = null
                    , bool enableStatistics = false
                    , int commandTimeoutInSeconds = 90
                );

        Task
            ExecuteReaderProcessAsync
                (
                    string connectionString
                    , string storeProcedureName
                    , JToken parameters = null
                    , Func
                        <
                            int             // resultset Index
                            , JArray        // columns
                            , IDataReader
                            , int           // row index
                            , Task
                        > onReadRowProcessActionAsync = null
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
                ExecuteJsonResultsAsync
                    (
                        string connectionString
                        , string storeProcedureName
                        , JToken parameters = null
                        , Func
                            <
                                int             // resultSet index
                                , IDataReader
                                , int           // row index
                                , int           // column index
                                , Type          // fieldType
                                , string        // fieldName
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
//#endif