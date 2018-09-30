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
                    , JToken parameters
                    , out JToken result
                    , Func
                        <
                            IDataReader
                            , Type        // fieldType
                            , string    // fieldName
                            , int       // row index
                            , int       // column index
                            , JProperty   //  JObject Field 对象
                        > onReadRowColumnProcessFunc = null
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