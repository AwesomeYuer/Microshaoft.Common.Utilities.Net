#if NET45
namespace Microsoft.Boc
{
    //using Microsoft.Boc;
    using Microsoft.Boc.Share;
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.MEF.CompositionContainers;
    using System;
    using System.Net;
    public static class MessagesReceiveProcessors
    {
        private static MessagesReceiveProcessorsCompositionContainer _messagesReceiveProcessorsCompositionContainer
                = new MessagesReceiveProcessorsCompositionContainer();

        public static void Load()
        {
            _messagesReceiveProcessorsCompositionContainer
                .ImportManyExports
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .MessagesReceiveProcessorsPartsPluginsPath
                    );
        }
        public static void ProcessDataPack
                                (
                                    string tag
                                    , Tuple
                                        <
                                            SocketAsyncDataHandler<SessionContextEntry>
                                            , EndPoint
                                            , IMessage
                                            , string    //Xml/Json
                                        > data
                                )
        {
            _messagesReceiveProcessorsCompositionContainer.InvokeOnePartProcessFunc(tag, data);
        }
    }
}
#endif