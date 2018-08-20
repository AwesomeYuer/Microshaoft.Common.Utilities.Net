namespace Microshaoft.Web
{
    public class StoreProceduresExecuteService
                            : AbstractStoreProceduresService
    {
        public StoreProceduresExecuteService()
        {
            base
                .LoadDataBasesConfiguration("dbConnections.json");
            base
                .LoadDynamicExecutors("dynamicLoadExecutorsPaths.json"); ;
        }
        protected override int
                CachedExecutingParametersDefinitionExpiredInSeconds => 11;
        protected override bool 
                NeedAutoRefreshExecutedTimeForSlideExpire => true;
    }
}
