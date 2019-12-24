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
        public static bool TryGetValue<T>
                        (
                            this IConfiguration target
                            , string sectionKey
                            , out T sectionValue
                        )
        {
            var r = TryGetSection
                        (
                            target
                            , sectionKey
                            , out _
                        );
            if (r)
            {
                r = false;
                sectionValue = target
                                    .GetValue<T>
                                        (
                                            sectionKey
                                        );
                r = true;
            }
            else
            {
                sectionValue = default;
            }
            return r;
        }
    }
}