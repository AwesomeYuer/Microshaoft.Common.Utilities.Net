namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Microsoft.Data.Sqlite;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class SqliteStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <SqliteConnection, SqliteCommand, SqliteParameter>
    {
        public AbstractStoreProceduresExecutor
                    <SqliteConnection, SqliteCommand, SqliteParameter>
                        _executor = new SqliteStoreProceduresExecutor();

        public override AbstractStoreProceduresExecutor<SqliteConnection, SqliteCommand, SqliteParameter> Executor
        {
            get => _executor;
            set => _executor = value;
        }

        public override string DataBaseType
        {
            get => "sqlite";
        }
    }
}
