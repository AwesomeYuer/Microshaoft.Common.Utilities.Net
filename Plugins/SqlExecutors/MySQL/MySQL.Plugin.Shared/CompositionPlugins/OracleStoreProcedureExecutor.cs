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
        private object _locker = new object();
        public override void InitializeInvokingCachingStore(ConcurrentDictionary<string, ExecutingInfo> executingCachingStore)
        {
            _locker
                .LockIf
                    (
                        () =>
                        {
                            return
                                (_executor == null);
                        }
                        , () =>
                        {
                            _executor = new OracleStoreProceduresExecutor(executingCachingStore);
                        }
                    );
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
