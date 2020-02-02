namespace Microshaoft.Web
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Threading.Tasks;

    public interface IStoreProceduresService
    {
        (
            bool Success
            , JToken Result
            , TimeSpan? DbExecutingDuration
        )
               Process
                       (
                           string connectionString
                           , string dataBaseType
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
        Task
            <
                (
                    bool Success
                    , JToken Result
                    , TimeSpan? DbExecutingDuration
                )
            >
               ProcessAsync
                       (
                           string connectionString
                           , string dataBaseType
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
}

