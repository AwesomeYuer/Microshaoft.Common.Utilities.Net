#if NET45
namespace Microsoft.Boc
{
    //using Microsoft.Boc;
    using Microsoft.Boc.Share;
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.MEF.CompositionContainers;
    using System;
    using System.Net;
    public static class MessagesProcessors
    {
        private static MessagesCompositionContainer _messagesCompositionContainer
                = new MessagesCompositionContainer();

        public static void Load()
        {
            _messagesCompositionContainer
                .ImportManyExports
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .MessagesProcessorsPartsPluginsPath
                    );
        }

    }
}
#endif
