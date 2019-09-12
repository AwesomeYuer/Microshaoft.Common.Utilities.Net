namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Oracle.ManagedDataAccess.Client;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class OracleStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <OracleConnection, OracleCommand, OracleParameter>
    {
        public AbstractStoreProceduresExecutor
                    <OracleConnection, OracleCommand, OracleParameter>
                        _executor = new OracleStoreProceduresExecutor();

        public override AbstractStoreProceduresExecutor<OracleConnection, OracleCommand, OracleParameter> Executor
        {
            get => _executor;
            set => _executor = value;
        }

        public override string DataBaseType
        {
            get => "oracle";
        }
    }
}
