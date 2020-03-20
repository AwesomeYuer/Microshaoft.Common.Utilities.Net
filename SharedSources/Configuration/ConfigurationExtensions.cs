namespace Microshaoft
{
    using Microsoft.Extensions.Configuration;
    public static class ConfigurationExtensions
    {
      public static bool TryGetSection
                    (
                        this IConfiguration @this
                        , string sectionKey
                        , out IConfigurationSection section
                    )
        {
            if (sectionKey == null)
            {
                section = (IConfigurationSection) @this;
            }
            else
            {
                section = @this
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
        // only for Array Value
        public static T GetOrDefault<T>
                            (
                                this IConfiguration @this
                                , string sectionKey
                                , T defaultValue = default
                            )
        {
            T r = defaultValue;
            var b = TryGet
                        (
                            @this
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
                            this IConfiguration @this
                            , string sectionKey
                            , out T sectionValue
                        )
        {
            var r = TryGetSection
                        (
                            @this
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