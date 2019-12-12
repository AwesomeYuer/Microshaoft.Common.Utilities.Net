namespace Microshaoft
{
    using Microsoft.Extensions.Configuration;
    using System;

    public static class ConfigurationExtensions
    {
        public static bool TryGetSection
                            (
                                this IConfiguration target
                                , string sectionKey = null
                                , Action<IConfiguration, IConfigurationSection>
                                        ifExistsProcessAction = null
                            )
        {
            IConfigurationSection section;
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
            if (r)
            {

                ifExistsProcessAction?
                                    .Invoke
                                        (
                                            target
                                            , section
                                        );
            }
            return r;
        }
    }
}
