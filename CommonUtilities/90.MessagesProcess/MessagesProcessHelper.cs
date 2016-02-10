namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.Share;
    using System;
    using System.Text;
    public static class MessagesProcessHelper
    {
        /// <summary>
        /// 向Client发送回执
        /// </summary>
        /// <param name="socketAsyncDataHandler"></param>
        /// <param name="getOriginalRequestFunc"></param>
        /// <param name="getResultFunc"></param>
        public static void SendCommonResponseOnceDirectly
                       (
                           IMessage requestMessage
                           , SocketAsyncDataHandler<SessionContextEntry> socketAsyncDataHandler
                           , SessionContextEntry sessionContextEntry
                       )
        {
            var requestMessageHeader = requestMessage.Header;
            CommonResponse commonResponse = new CommonResponse()
            {
                Header = new MessageHeader()
                {
                      SendCount = 1
                      , Sender = null
                      , ID = 0 //SequenceSeedHelper.NewID()
                      , LinkID = requestMessage.Header.ID
                      , Receivers = new Party[]
                                        {
                                            requestMessageHeader.Sender
                                        }
                      , RequestTopic = requestMessageHeader.Topic
                      , RequireResponse = 0
                      , ResultValue = 0
                      , SendTimeStamp = DateTime.Now
                      , Topic = "CommonResponse"
                }
                , Body = null
            };
            var remoteIPEndPoint = sessionContextEntry.OwnerRemoteIPEndPoint;
            var json = JsonHelper.Serialize(commonResponse);
            var buffer = Encoding.UTF8.GetBytes(json);
            var enabledSendedMessagesPerformanceCounters = MultiPerformanceCountersTypeFlags.None;
            if 
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableCountPerformance
                )
            {
                enabledSendedMessagesPerformanceCounters
                    = ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnabledSendedMessagesPerformanceCounters;
            }
            socketAsyncDataHandler
                .SendDataToSyncWithPerformaceCounter
                    (
                        remoteIPEndPoint
                        , buffer
                        , enabledSendedMessagesPerformanceCounters
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .NeedThrottleControlSendHeartBeatsRequestOrResponse
                    );
        }
    }
}