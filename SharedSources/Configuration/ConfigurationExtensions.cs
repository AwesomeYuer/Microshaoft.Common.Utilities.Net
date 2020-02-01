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
        public static T GetOrDefault<T>
                            (
                                this IConfiguration target
                                , string sectionKey
                                , T defaultValue = default
                            )
        {
            T r = defaultValue;
            var b = TryGet
                        (
                            target
                            , sectionKey
                            , out T @value
                        );
            if (b)
            {
                r = @value;
            }
            return r;
        }


        public static bool TryGet<T>
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
                            , out var configuration
                        );
            if (r)
            {
                r = false;
                sectionValue = configuration
                                        .Get<T>();
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