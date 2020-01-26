namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using MySql.Data.MySqlClient;
    using System.Collections.Concurrent;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class MySQLStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <MySqlConnection, MySqlCommand, MySqlParameter>
    {
        public AbstractStoreProceduresExecutor
                    <MySqlConnection, MySqlCommand, MySqlParameter>
                        _executor;
        private object _locker = new object();
        public override void InitializeOnDemand
                                    (
                                        ConcurrentDictionary<string, ExecutingInfo>
                                            executingCachingStore
                                    )
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
                            _executor = new MySqlStoreProceduresExecutor(executingCachingStore);
                        }
                    );
        }

        public override AbstractStoreProceduresExecutor<MySqlConnection, MySqlCommand, MySqlParameter> Executor
        {
            get => _executor;
        }
        public override string DataBaseType
        {
            get => "mysql";
        }
    }
}
