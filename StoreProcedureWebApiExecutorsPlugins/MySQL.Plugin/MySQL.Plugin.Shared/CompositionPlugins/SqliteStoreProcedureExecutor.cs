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

        public override void InitializeOnDemand
                                (
                                    ConcurrentDictionary<string, ExecutingInfo>
                                        executingCachingStore
                                )
        {
            _executor = new SqliteStoreProceduresExecutor(executingCachingStore);
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
