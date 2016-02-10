#if NET45
namespace Microsoft.Boc
{
    using Microsoft.Boc.Share;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public static partial class SessionsManager
    {
        public static IEnumerable
                            <
                                KeyValuePair
                                    <
                                        string
                                        , Tuple
                                            <
                                                SessionContextEntry
                                                , SocketAsyncDataHandler<SessionContextEntry>
                                            >
                                    >
                            > GetSessions
                                    (
                                        Party owner
                                        , string sessionHostServer
                                        , int skip
                                        , int take
                                    )
        {
            return
                GetSessions
                (
                    owner.AppID
                    , owner.GroupID
                    , owner.UserID
                    , sessionHostServer
                    , skip
                    , take
                );
        }

        public static IEnumerable
                            <
                                KeyValuePair
                                    <
                                        string
                                        , Tuple
                                            <
                                                SessionContextEntry
                                                , SocketAsyncDataHandler<SessionContextEntry>
                                            >
                                    >
                            > GetSessions
                                    (
                                        string appID
                                        , string groupID
                                        , string userID
                                        , string sessionHostServer
                                        , int skip
                                        , int take
                                    )
        {
            return
                _sessions
                    .Where
                        (
                            (x) =>
                            {
                                var sessionContextEntry = x.Value.Item1;
                                var a = new Tuple<string, string>[]
                                            {
                                                Tuple.Create<string,string>(appID, sessionContextEntry.OwnerAppID)
                                                , Tuple.Create<string,string>(groupID, sessionContextEntry.OwnerGroupID)
                                                , Tuple.Create<string,string>(userID, sessionContextEntry.OwnerUserID)
                                                , Tuple.Create<string,string>(sessionHostServer, sessionContextEntry.SessionHostServer)
                                            };
                                var r = StringsHelper.StringsCompareWithWild(a);
                                return r;
                            }
                        )
                    .Skip(skip)
                    .Take(take);
        }

        public static IEnumerable<SessionContextEntry> GetSessionsContextEntries
                                                            (
                                                                string appID
                                                                , string groupID
                                                                , string userID
                                                                , string sessionHostServer
                                                                , int skip
                                                                , int take
                                                            )
        {
            return
                _sessions
                    .Where
                        (
                            (x) =>
                            {
                                var sessionContextEntry = x.Value.Item1;
                                var a = new Tuple<string, string>[]
                                            {
                                                Tuple.Create<string,string>(appID, sessionContextEntry.OwnerAppID)
                                                , Tuple.Create<string,string>(groupID, sessionContextEntry.OwnerGroupID)
                                                , Tuple.Create<string,string>(userID, sessionContextEntry.OwnerUserID)
                                                , Tuple.Create<string,string>(sessionHostServer, sessionContextEntry.SessionHostServer)
                                            };
                                var r = StringsHelper.StringsCompareWithWild(a);
                                return r;
                            }
                        )
                    .Skip(skip)
                    .Take(take)
                    .Select
                        (
                            (x) =>
                            {
                                var sessionContextEntry = x.Value.Item1;
                                return sessionContextEntry;
                            }
                        );
        }
    }
}
#endif