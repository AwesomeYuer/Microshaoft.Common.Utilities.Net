#if IBM_Data_DB2_Core
namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using System.Composition;
    using IBM.Data.DB2.Core;
    using System.Collections.Concurrent;

    [Export(typeof(IStoreProcedureExecutable))]
    public class DB2StoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <DB2Connection, DB2Command, DB2Parameter>
    {
        public AbstractStoreProceduresExecutor
                    <DB2Connection, DB2Command, DB2Parameter>
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
                            _executor = new DB2StoreProceduresExecutor(executingCachingStore);
                        }
                    );
        }

        public override AbstractStoreProceduresExecutor<DB2Connection, DB2Command, DB2Parameter> Executor
        {
            get => _executor;
        }

        public override string DataBaseType
        {
            get => "db2";
        }
    }
}
#endif