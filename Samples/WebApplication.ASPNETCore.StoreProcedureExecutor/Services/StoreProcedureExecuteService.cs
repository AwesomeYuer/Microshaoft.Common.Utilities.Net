namespace Microshaoft.Web
{
    using Microsoft.Extensions.Configuration;
    public class StoreProceduresExecuteService
                            : AbstractStoreProceduresService
    {
        public StoreProceduresExecuteService(IConfiguration configuration)
                    : base(configuration)
        {
        }
    }
}
