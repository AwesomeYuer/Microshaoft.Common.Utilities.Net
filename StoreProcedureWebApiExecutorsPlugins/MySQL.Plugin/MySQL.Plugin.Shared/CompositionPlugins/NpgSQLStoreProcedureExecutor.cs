namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Npgsql;
    using System.Collections.Concurrent;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class NpgSQLStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
    {
        public AbstractStoreProceduresExecutor
                    <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
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
                           _executor = new NpgSqlStoreProceduresExecutor(executingCachingStore);
                       }
                   );
        }
        public override AbstractStoreProceduresExecutor<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter> Executor
        {
            get => _executor;
        }

        public override string DataBaseType
        {
            get => "npgsql";
        }
    }
}
