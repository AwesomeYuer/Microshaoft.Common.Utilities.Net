namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
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
                    , int commandTimeoutInSeconds = 90
                );
    }
    public interface ICacheAutoRefreshable
    {
        int CachedExpiredInSeconds
        {
            get;
            set;
        }
        bool NeedAutoRefreshForSlideExpire
        {
            get;
            set;
        }
    }
}