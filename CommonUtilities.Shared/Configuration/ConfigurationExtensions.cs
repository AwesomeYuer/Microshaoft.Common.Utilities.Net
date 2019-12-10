namespace Microshaoft
{
    using Microsoft.Extensions.Configuration;
    using System;

    public static class ConfigurationExtensions
    {
        public static bool ProcesSectionIfExists
                            (
                                this IConfiguration target
                                , string sectionKey = null
                                , Action<IConfiguration, IConfigurationSection> processIfExistsAction = null
                            )
        {
            var r = false;
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
            if (section.Exists())
            {
                r = true;
                processIfExistsAction?
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