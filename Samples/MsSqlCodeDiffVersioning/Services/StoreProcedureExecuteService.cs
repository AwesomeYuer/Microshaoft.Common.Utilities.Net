namespace Microshaoft.Web
{
    public class StoreProceduresExecuteService
                            : AbstractStoreProceduresService
    {
        //called by base AbstractStoreProceduresService constructor
        public override void Initialize()
        {
            //using derived class implmention
            LoadDataBasesConnectionsInfo("dbConnections.json");
            //using base class implmention
            base
                .LoadDynamicExecutors("dynamicLoadExecutorsPaths.json");
        }
        protected override void LoadDataBasesConnectionsInfo(string dbConnectionsJsonFile = "dbConnections.json")
        {
            // test for override base LoadDataBasesConnectionsInfo implement
            // you can implement by using other process except config json file(that's base method)
            base
                .LoadDataBasesConnectionsInfo(dbConnectionsJsonFile);
        }
        protected override int
                CachedParametersDefinitionExpiredInSeconds => 11;
        protected override bool 
                NeedAutoRefreshExecutedTimeForSlideExpire => true;
    }
}
