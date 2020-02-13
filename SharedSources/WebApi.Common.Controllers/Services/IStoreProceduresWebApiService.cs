namespace Microshaoft.Web
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Threading.Tasks;

    public interface IStoreProceduresWebApiService
    {
        (
            int StatusCode
            , string Message
            , JToken JResult
            , TimeSpan? DbExecutingDuration
        )
            Process
                (
                    string routeName
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
                    , string httpMethod = "Get"
                    //, bool enableStatistics = false
                    , int commandTimeoutInSeconds = 101
                );

        Task
            <
                (
                    int StatusCode
                    , string Message
                    , JToken JResult
                    , TimeSpan? DbExecutingDuration
                )
            >
                ProcessAsync
                    (
                        string routeName
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
                        , string httpMethod = "Get"
                        //, bool enableStatistics = false
                        , int commandTimeoutInSeconds = 101
                    );
    }
}

