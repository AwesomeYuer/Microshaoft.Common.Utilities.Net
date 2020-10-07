namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Microsoft.Data.Sqlite;
    using System.Collections.Concurrent;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class SqliteStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <SqliteConnection, SqliteCommand, SqliteParameter>
    {
        public AbstractStoreProceduresExecutor
                    <SqliteConnection, SqliteCommand, SqliteParameter>
                        _executor;
        private object _locker = new object();
        public override void InitializeInvokingCachingStore
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
                            _executor = new SqliteStoreProceduresExecutor(executingCachingStore);
                        }
                    );
        }

        public override AbstractStoreProceduresExecutor<SqliteConnection, SqliteCommand, SqliteParameter> Executor
        {
            get => _executor;
        }

        public override string DataBaseType
        {
            get => "sqlite";
        }
    }
}
