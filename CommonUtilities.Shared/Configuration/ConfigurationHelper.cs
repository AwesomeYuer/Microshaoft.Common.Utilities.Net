namespace Microshaoft
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationHelper
    {
        private static object _locker = new object();
        public static IConfiguration Configuration
        {
            get;
            private set;
        }
        public static void Load(IConfiguration configuration)
        {
            _locker
                .LockIf
                    (
                        () =>
                        {
                            return
                                Configuration == null;
                        }
                        , () =>
                        {
                            Configuration = configuration;
                        }
                    );
        }
    }
}