namespace Microsoft.Boc.Share
{
    using Newtonsoft.Json.Linq;
    public class MessageSendEntry
    {
        public Party Sender;
        public Party ReceiverEntry;
        public int ReceiverEntryID;
        public long? MessageID;
        public JObject MessageJObject;
        public bool CanForward;
    }
}
