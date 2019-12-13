namespace Microshaoft
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static bool TryGetSection
                    (
                        this IConfiguration target
                        , string sectionKey
                        , out IConfigurationSection section
                    )
        {
            if (sectionKey == null)
            {
                section = (IConfigurationSection) target;
            }
            else
            {
                section = target
                                .GetSection
                                    (
                                        sectionKey
                                    );
            }
            var r = section.Exists();
            if (!r)
            {
                section = null;
            }
            return r;
        }
    }
}