namespace Microshaoft.Web
{
    using Microsoft.Extensions.Configuration;
    using System.Collections.Concurrent;

    public class StoreProceduresExecuteService
                            : AbstractStoreProceduresService
    {
        public StoreProceduresExecuteService
                        (
                            IConfiguration configuration
                            , ConcurrentDictionary<string, ExecutingInfo>
                                        executingCachingStore
        
                        )
                    : base(configuration, executingCachingStore)
        {
        }
    }
}
