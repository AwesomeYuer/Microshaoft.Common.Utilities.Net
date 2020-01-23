#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Runtime.Serialization.Formatters;

    public static class RemotingHelper
    {
        public static void StartRemoting
            (
                Type RemotingType
                , string Url
                , int Port
                , WellKnownObjectMode ServiceMode
            )
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary ht = new Hashtable();
            ht["port"] = Port;
            TcpChannel tc = new TcpChannel(ht, null, provider);
            ChannelServices.RegisterChannel(tc, false);
            RemotingConfiguration.RegisterWellKnownServiceType(RemotingType, Url, ServiceMode);
            Console.WriteLine("Remoting Object Started ...");
        }
        public static void StartRemoting<T>
            (
                string Url
                , int Port
                , WellKnownObjectMode Mode
            )
        {
            StartRemoting(typeof(T), Url, Port, Mode);
        }
        public static T GetRemotingLocalClientProxyObject<T>
            (
                string Url
            )
        {
            return (T)Activator.GetObject
                                    (
                                        typeof(T)
                                        , Url
                                    );
        }
    }
}

#endif