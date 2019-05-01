namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Npgsql;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class NpgSQLStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
    {
        public AbstractStoreProceduresExecutor
                    <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
                        _executor = new NpgSqlStoreProceduresExecutor();

        public override AbstractStoreProceduresExecutor<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter> Executor
        {
            get => _executor;
            set => _executor = value;
        }

        public override string DataBaseType
        {
            get => "npgsql";
        }
    }
}
