namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Oracle.ManagedDataAccess.Client;
    using System.Collections.Concurrent;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class OracleStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <OracleConnection, OracleCommand, OracleParameter>
    {
        public AbstractStoreProceduresExecutor
                    <OracleConnection, OracleCommand, OracleParameter>
                        _executor;

        public override void InitializeOnDemand(ConcurrentDictionary<string, ExecutingInfo> store)
        {
            _executor = new OracleStoreProceduresExecutor(store);
        }

        public override AbstractStoreProceduresExecutor<OracleConnection, OracleCommand, OracleParameter> Executor
        {
            get => _executor;
        }

        public override string DataBaseType
        {
            get => "oracle";
        }
    }
}
