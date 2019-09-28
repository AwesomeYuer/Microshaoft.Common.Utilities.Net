namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using System.Composition;
    using System.Data.SqlClient;

    [Export(typeof(IStoreProcedureExecutable))]
    public class MsSQLStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <SqlConnection, SqlCommand, SqlParameter>
                        //: IStoreProcedureExecutable
                        //    , IParametersDefinitionCacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <SqlConnection, SqlCommand, SqlParameter>
                        _executor = new MsSqlStoreProceduresExecutor();

        public override AbstractStoreProceduresExecutor<SqlConnection, SqlCommand, SqlParameter> Executor
        {
            get => _executor;
            set => _executor = value;
        }

        public override string DataBaseType
        {
            get => "mssql";
        }
        protected override void BeforeExecutingProcess
                        (
                            string connectionString
                            , bool enableStatistics
                            , out SqlConnection connection
                        )
        {         
            base
                .BeforeExecutingProcess
                    (
                        connectionString
                        , enableStatistics
                        , out connection
                    );
            if (enableStatistics)
            {
                connection.StatisticsEnabled = enableStatistics;
            }
        }
    }
}
