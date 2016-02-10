
namespace Microshaoft.MEF.CompositionContainers
{
    using Boc;
    using Contracts;
    using Share;
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Net;
   
    public class MessagesReceiveProcessorsCompositionContainer
                    : IMefPartsCompositionContainer
                            <
                                IMefChainedProcessorPart
                                    <
                                        MessagesReceiveProcessorsCompositionContainer
                                        , string
                                        , string
                                        , Tuple
                                            <
                                                SocketAsyncDataHandler<SessionContextEntry>
                                                , EndPoint
                                                , IMessage
                                                , string
                                            >
                                    >
                                , string
                                , string
                                , Tuple
                                    <
                                        SocketAsyncDataHandler<SessionContextEntry>
                                        , EndPoint
                                        , IMessage
                                        , string
                                    >
                            >
    {
        [
            ImportMany
                (
                    typeof
                        (
                            IMefChainedProcessorPart
                                        <
                                            MessagesReceiveProcessorsCompositionContainer
                                            , string
                                            , string
                                            , Tuple
                                                <
                                                    SocketAsyncDataHandler<SessionContextEntry>
                                                    , EndPoint
                                                    , IMessage
                                                    , string
                                                >
                                        >
                         )
                )
        ]
        public IMefChainedProcessorPart
                                    <
                                        MessagesReceiveProcessorsCompositionContainer
                                        , string
                                        , string
                                        , Tuple
                                            <
                                                SocketAsyncDataHandler<SessionContextEntry>
                                                , EndPoint
                                                , IMessage
                                                , string
                                            >
                                    >
                    [] Parts
        {
            get;
            private set;
        }
        ConcurrentDictionary
                    <
                        string
                        , IMefChainedProcessorPart
                                    <
                                        MessagesReceiveProcessorsCompositionContainer
                                        , string
                                        , string
                                        , Tuple
                                            <
                                                SocketAsyncDataHandler<SessionContextEntry>
                                                , EndPoint
                                                , IMessage
                                                , string
                                            >
                                    >
                    > _dictionary;
        public void ImportManyExports(string path)
        {
            MEFHelper
                .ImportManyExportsComposeParts
                    <MessagesReceiveProcessorsCompositionContainer>
                        (
                            path
                            , this
                        );
            var result = Parts.OrderBy(x => x.Priority);
            if (_dictionary == null)
            {
                _dictionary = new ConcurrentDictionary
                                    <
                                        string
                                        , IMefChainedProcessorPart
                                            <
                                                MessagesReceiveProcessorsCompositionContainer
                                                , string
                                                , string
                                                , Tuple
                                                    <
                                                        SocketAsyncDataHandler<SessionContextEntry>
                                                        , EndPoint
                                                        , IMessage
                                                        , string
                                                    >
                                            >
                                     >();
            }
            result
                .ToList()
                .ForEach
                    (
                        x
                        =>
                        {
                            _dictionary[x.Key.ToLower().Trim()] = x;
                        }
                    );
        }

        public string InvokeOnePartProcessFunc
                            (
                                string PartKey
                                , params
                                        Tuple
                                            <
                                                SocketAsyncDataHandler<SessionContextEntry>
                                                , EndPoint
                                                , IMessage
                                                , string
                                            >
                                                [] parameters
                            )
        {
            //PartKey = PartKey.ToLower().Trim();
            string r = string.Empty;
            var parameter = parameters[0];
            var sender = parameter
                           .Item3
                           .Header
                           .Sender;
            if (sender != null)
            {
                if
                    (
                        !string.IsNullOrEmpty(sender.AppID)
                        &&
                        !string.IsNullOrEmpty(sender.GroupID)
                        &&
                        !string.IsNullOrEmpty(sender.UserID)
                    )
                {
                    IMefChainedProcessorPart
                        <
                            MessagesReceiveProcessorsCompositionContainer
                            , string
                            , string
                            , Tuple
                                <
                                    SocketAsyncDataHandler<SessionContextEntry>
                                    , EndPoint
                                    , IMessage
                                    , string
                                >
                        > part;
                    if
                        (
                            _dictionary
                                .TryGetValue(PartKey, out part)
                        )
                    {
                        r = part.OnOnceProcessFunc(parameter);
                    }
                    else
                    {
                        //var sessionContextEntry = parameter.Item1;
                        //var socketAsyncDataHandler = parameter.Item1;
                        //CommonResponseHelper.SendCommonResponse
                        //    (
                        //        handler
                        //        , null
                        //        , () =>
                        //            {
                        //                return
                        //                new ResultInfo()
                        //                {
                        //                    ReturnValue = -10001
                        //                     ,
                        //                    Description = "非法的数据包, 即将关闭该连接"
                        //                     ,
                        //                    ErrorCode = "-10001"
                        //                };
                        //            }
                        //    );
                        //var session = handler.Token;
                        //SessionsManager.RemoveAndDestoryUserConnection
                        //        (
                        //            session.User
                        //            , handler.SocketID
                        //        );
                        //var socketID = socketAsyncDataHandler.SocketID;
                        //string removedUserID = string.Empty;
                        //int removedSocketID = 0;
                        //SessionsManager.RemoveUserConnectionBySocketID
                        //                    (
                        //                        socketID
                        //                        , true      //只删除 Socket
                        //                        , out removedSocketID
                        //                        , out removedUserID
                        //                        , (x, y) =>
                        //                            {
                        //                                y.DestoryWorkingSocket();
                        //                            }
                        //                    );

                    }

                }
            }
            return r;
        }
        public void ChainedInvokeAllPartsProcessAction(params Tuple<SocketAsyncDataHandler<SessionContextEntry>, EndPoint, IMessage, string>[] parameters)
        {
            throw new NotImplementedException();
        }

        public string ChainedInvokeAllPartsProcessFunc(params Tuple<SocketAsyncDataHandler<SessionContextEntry>, EndPoint, IMessage, string>[] parameters)
        {
            throw new NotImplementedException();
        }

        public void InvokeOnePartProcessAction(string PartKey, params Tuple<SocketAsyncDataHandler<SessionContextEntry>, EndPoint, IMessage, string>[] parameters)
        {
            throw new NotImplementedException();
        }

        public void ChainedInvokeAllPartsProcessAction(params Tuple<SocketAsyncDataHandler<SessionContextEntry>, EndPoint, MessageHeader, string>[] parameters)
        {
            throw new NotImplementedException();
        }

        public string ChainedInvokeAllPartsProcessFunc(params Tuple<SocketAsyncDataHandler<SessionContextEntry>, EndPoint, MessageHeader, string>[] parameters)
        {
            throw new NotImplementedException();
        }

        public void InvokeOnePartProcessAction(string PartKey, params Tuple<SocketAsyncDataHandler<SessionContextEntry>, EndPoint, MessageHeader, string>[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
