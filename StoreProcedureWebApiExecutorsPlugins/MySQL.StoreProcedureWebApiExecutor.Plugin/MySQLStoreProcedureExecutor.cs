namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using MySql.Data.MySqlClient;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class MySQLStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <MySqlConnection, MySqlCommand, MySqlParameter>
    {
        public AbstractStoreProceduresExecutor
                    <MySqlConnection, MySqlCommand, MySqlParameter>
                        _executor = new MySqlStoreProceduresExecutor();

        public override AbstractStoreProceduresExecutor<MySqlConnection, MySqlCommand, MySqlParameter> Executor
        {
            get => _executor;
            set => _executor = value;
        }

        public override string DataBaseType
        {
            get => "mysql";
        }
    }
}
