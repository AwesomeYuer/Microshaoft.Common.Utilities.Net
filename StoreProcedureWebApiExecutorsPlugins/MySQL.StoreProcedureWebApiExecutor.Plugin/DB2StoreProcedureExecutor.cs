namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using System.Composition;
    using IBM.Data.DB2.Core;

    [Export(typeof(IStoreProcedureExecutable))]
    public class DB2StoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <DB2Connection, DB2Command, DB2Parameter>
    {
        public AbstractStoreProceduresExecutor
                    <DB2Connection, DB2Command, DB2Parameter>
                        _executor = new DB2StoreProceduresExecutor();

        public override AbstractStoreProceduresExecutor<DB2Connection, DB2Command, DB2Parameter> Executor
        {
            get => _executor;
            set => _executor = value;
        }

        public override string DataBaseType
        {
            get => "db2";
        }
    }
}
