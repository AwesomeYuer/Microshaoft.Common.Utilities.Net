namespace Microsoft.Boc.Share
{
    using Microsoft.Boc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Web.Script.Serialization;
    using System.Xml.Serialization;

    //XML Json 互斥 Ignore
    [JsonObject(MemberSerialization.OptIn)]
    public class MessageTransmissionTrackerEntry
    {
        #region Ignore 字段
        [ScriptIgnore]
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public SessionContextEntry ReceiverSessionContextEntry;

        [ScriptIgnore]
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public SocketAsyncDataHandler<SessionContextEntry>
                        ReceiverSocketAsyncDataHandler;

        [ScriptIgnore]
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public Stopwatch Stopwatcher;

        [ScriptIgnore]
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public AutoResetEvent ResponseWaiter;

        [ScriptIgnore]
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public Func<MessageTransmissionTrackerEntry, Tuple<bool, bool>> 
                    OnSetAutoResetEventProcessFunc;
        
        #endregion


        #region XML Json 互斥 Ignore
        //[ScriptIgnore]
        [XmlIgnore]
        //[JsonIgnore]
        [JsonProperty]
        [IgnoreDataMember]
        public JObject MessageJObject;

        [ReadOnly(true)]
        [JsonIgnore]
        [DataMember]
        public string MessageJson
        {
            get
            {
                var r = string.Empty;
                if (MessageJObject != null)
                {
                    r = JsonHelper.Serialize(MessageJObject);
                }
                return r;
            }
            set { }
        } 
        #endregion

        #region 内部跟踪使用 + WebAPI 查询返回 字段
        [JsonProperty]
        public Party Sender;
        [JsonProperty]
        public long? MessageID;
        [JsonProperty]
        public int ReceiverEntryID;
        [JsonProperty]
        public DateTime? FirstTransmissionTime;
        [JsonProperty]
        public DateTime? LastTransmissionTime;
        [JsonProperty]
        public int TransmissionTimes;
        [JsonProperty]
        public DateTime? RequestTime;
        [JsonProperty]
        public string ResponseWaiterPairID;
        [JsonProperty]
        public bool ContinueWaiting = true;
        [JsonProperty]
        public int? Status = 0;
        [JsonProperty]
        public DateTime? ResponsedTime;
        [JsonProperty]
        public bool IsResponsed = false;
        #endregion

        #region WebAPI 查询返回只读字段
        [ReadOnly(true)]
        [JsonProperty]
        public Party Receiver
        {
            get
            {
                Party r = null;
                if (ReceiverSessionContextEntry != null)
                {
                    if (ReceiverSessionContextEntry.Owner != null)
                    {
                        r = ReceiverSessionContextEntry.Owner;
                    }
                }
                return r;
            }
            set { }
        }


        [ReadOnly(true)]
        [JsonProperty]
        public string RemoteIPEndPoint
        {
            get
            {
                string r = string.Empty;
                if (ReceiverSessionContextEntry != null)
                {
                    if (ReceiverSessionContextEntry.Owner != null)
                    {
                        r = ReceiverSessionContextEntry.OwnerRemoteIPEndPointToString;
                    }
                }
                return r;
            }
            set { }
        }
        [ReadOnly(true)]
        [JsonProperty]
        public string SessionHostServer
        {
            get
            {
                string r = string.Empty;
                if (ReceiverSessionContextEntry != null)
                {
                    if (ReceiverSessionContextEntry.Owner != null)
                    {
                        r = ReceiverSessionContextEntry.SessionHostServer;
                    }
                }
                return r;
            }
            set { }
        }

        
        [ReadOnly(true)]
        [JsonProperty]
        public long? WaitingInSeconds
        {
            get
            {
                var r = -1L;
                if (Stopwatcher != null)
                {
                    r = Stopwatcher
                            .ElapsedMilliseconds
                        / 1000;
                }
                return
                    r;
            }
            set { }
        } 
        #endregion
    }
}
//namespace Microsoft.Boc.Share
//{
//    using Microsoft.Boc.Share;
//    //using System.Web.Cors;
//    using Newtonsoft.Json.Linq;
//    using System;
//    public class JsonMessageTransmissionTrackerEntry1
//    {
//        //public Party Sender;
//        //public Party Receiver;
//        //public SessionContextEntry ReceiverSessionContextEntry;
//        //public SocketAsyncDataHandler<SessionContextEntry> ReceiverSocketAsyncDataHandler;
//        public long? MessageID;
//        //public long? MessageIDForOneReceiverEntry;
//        public JObject Message;
//        //public string MessageDatagram;
//        //public Stopwatch Stopwatcher;
//        //public DateTime? LastTransmissionTime;
//        //public int TransmissionTimes;
//        //public DateTime? RequestTime;
//        //public AutoResetEvent ResponseWaiter;
//        //public string ResponseWaiterPairID;
//        //public bool ContinueWaiting = true;
//        //public int? Status = 0;
//        //public long? WaitingInSeconds;
//        public DateTime? ResponsedTime;
//        //public string LocalIPEndPoint;
//        public string RemoteIPEndPoint;
//        public string SessionHostServer;

//    }
//}