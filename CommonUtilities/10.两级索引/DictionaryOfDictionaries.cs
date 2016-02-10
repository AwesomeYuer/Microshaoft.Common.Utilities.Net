namespace Microsoft.Boc
{
    using Microsoft.Boc.Share;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    public static class SessionsIndex
    {
        public static ConcurrentDictionary
                                <
                                    string
                                    , ConcurrentDictionary
                                        <
                                            string
                                            , Tuple
                                                <
                                                    SessionContextEntry
                                                    , SocketAsyncDataHandler<SessionContextEntry>
                                                >
                                        >

                                > Index
                    = new ConcurrentDictionary
                                <
                                    string
                                    , ConcurrentDictionary
                                            <
                                                string
                                                , Tuple
                                                    <
                                                        SessionContextEntry
                                                        , SocketAsyncDataHandler<SessionContextEntry>
                                                    >
                                            >
                                >();


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
                            > 
                                GetSessionsByKey1
                                    (
                                        string key1
                                    )
        {
            IEnumerable
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
                    > result = null;
            ConcurrentDictionary
                    <
                        string
                        , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                    > data = null;
            if
                (
                    Index
                        .TryGetValue
                            (
                                key1
                                , out data
                            )
                )
            {
                result = data.AsEnumerable();
            }
            return result;
        }


        public static bool Add
                            (
                                Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                                    session
                            )
        {
            var r = false;
            string key1 = string.Format
                            (
                                "{1}{0}{2}"
                                , "-"
                                , session.Item1.OwnerAppID.Trim()
                                , session.Item1.OwnerGroupID.TrimStart('0', ' ')
                            ).Trim().ToLower();
            string key2 = session.Item1.SessionID;
            ConcurrentDictionary
                    <
                        string
                        , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                    > data
                        = Index
                            .GetOrAdd
                                (
                                    key1
                                    , (x) =>
                                        {
                                            ConcurrentDictionary
                                                <
                                                    string
                                                    , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                                                > rr
                                                    = new ConcurrentDictionary
                                                                <
                                                                    string
                                                                    , Tuple<SessionContextEntry,SocketAsyncDataHandler<SessionContextEntry>>
                                                                >();
                                            return rr;
                                        }
                                );
            //2014-09-25 必须使用 AddOrUpdate 更新索引 不能使用 TryAdd
            //data
            //    .AddOrUpdate
            //        (
            //            key2
            //            , session
            //            , (x, y) =>
            //                {
            //                    return session;
            //                }
            //        );

            if
                (
                    data.TryAdd(key2, session)
                )
            {
                r = true;
            }

            return r;
        }
        public static void Remove
                            (
                                Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                                    session
                            )
        {
            string key1 = string.Format
                            (
                                "{1}{0}{2}"
                                , "-"
                                , session.Item1.OwnerAppID.Trim()
                                , session.Item1.OwnerGroupID.TrimStart('0', ' ')
                            ).Trim().ToLower();
            string key2 = session.Item1.SessionID;
            ConcurrentDictionary
                <
                    string
                    , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                > data = null;
            if
                (
                    Index
                        .TryGetValue
                            (
                                key1
                                , out data
                            )
                )
            {
                Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                        removedSession = null;
                data.TryRemove
                        (
                           key2
                           , out removedSession
                        );
            }
        }
    }
}
