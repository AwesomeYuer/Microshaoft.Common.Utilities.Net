namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;

    public interface IStoreProcedureExecutable
    {
        string DataBaseType
        {
            get;
        }
        bool Execute
                (
                    string connectionString
                    , string storeProcedureName
                    , out JToken result
                    , JToken parameters = null
                    , Func
                        <
                            IDataReader
                            , Type        // fieldType
                            , string    // fieldName
                            , int       // row index
                            , int       // column index
                            , JProperty   //  JObject Field 对象
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