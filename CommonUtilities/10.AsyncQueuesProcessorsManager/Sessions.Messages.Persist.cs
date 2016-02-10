#if NET45
namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.Share;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Diagnostics;
    using System.Threading;
    public static partial class AsyncQueuesProcessorsManager
    {
        public static SingleThreadAsyncDequeueProcessor
                            <
                                Tuple
                                   <
                                        string                    //操做
                                        , SessionContextEntry
                                    >
                            > SessionsSyncToDataBaseAsyncQueueProcessor
            = new SingleThreadAsyncDequeueProcessor<Tuple<string, SessionContextEntry>>();
        private static DataTable _sessionsEmptyDataTable =
                    new Func<DataTable>
                        (
                            () =>
                            {
                                var type = typeof(SessionContextEntry);
                                var dataTable = DataTableHelper.GenerateEmptyDataTable(type, true);
                                return dataTable;
                            }
                        )();
        public static void RunSessionsSyncToDataBaseAsyncQueueProcess()
        {
            var dataTable = _sessionsEmptyDataTable.Clone();
            
            dataTable.Columns.Add("Action", typeof(string));
            dataTable.Columns.Add("ID", typeof(int));

            SessionsSyncToDataBaseAsyncQueueProcessor
                .StartRunDequeuesThreadProcess
                    (
                        null
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .SessionsSyncAsyncToDataBaseWaitSleepInMilliseconds
                        , (x, y) =>
                            {
                                try
                                {
                                    SessionsSyncSaveToDataBaseProcess
                                        (
                                            dataTable
                                            , y
                                        );
                                }
                                finally
                                {
                                    dataTable.Clear();
                                }
                            }
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .SessionsSyncAsyncToDataBaseWaitBatchTimeoutInSeconds * 1000
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .SessionsSyncAsyncToDataBaseWaitBatchMaxSizeInEntriesCount
                        , (x, y) =>
                            {
                                string s = string.Format
                                                (
                                                    "{1}{0}{2}"
                                                    , "\r\n"
                                                    , y
                                                    , x.ToString()
                                                );
                                Console.WriteLine(s);
                                EventLogHelper
                                    .WriteEventLogEntry
                                        (
                                            ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogSourceName
                                            , s
                                            , EventLogEntryType.Error
                                            , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionEventID
                                            , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionCategory
                                        );
                                return false;
                            }
                    );
        }

        private static void SessionsSyncSaveToDataBaseProcess
                                    (
                                        DataTable dataTable
                                        , List<Tuple<long, Tuple<string, SessionContextEntry>>> list
                                    )
        {
            var query =
            list
                .Where
                    (
                        (x) =>
                        {
                            var r =
                                    !list.Any
                                        (
                                            (xx) =>
                                            {
                                                var rr =
                                                        (xx.Item2.Item2.OwnerAppID == x.Item2.Item2.OwnerAppID)
                                                        &&
                                                        (xx.Item2.Item2.OwnerGroupID == x.Item2.Item2.OwnerGroupID)
                                                        &&
                                                        (xx.Item2.Item2.OwnerUserID == x.Item2.Item2.OwnerUserID)
                                                        &&
                                                        (
                                                            xx.Item1 > x.Item1
                                                        );
                                                return rr;
                                            }
                                        );
                            return r;
                        }
                    );
            foreach (var xx in query)
            {
                var action = xx.Item2.Item1;
                var sessionContextEntry = xx.Item2.Item2;
                var dataRow = dataTable.NewRow();
                dataRow["ID"] = xx.Item1;
                dataRow["Action"] = action;
                dataRow["OwnerAppID"] = sessionContextEntry.OwnerAppID;
                dataRow["OwnerGroupID"] = sessionContextEntry.OwnerGroupID;
                dataRow["OwnerUserID"] = sessionContextEntry.OwnerUserID;
                if (action.ToLower().Trim() != "delete")
                {
                    dataRow["SessionHostServer"] = sessionContextEntry.SessionHostServer;
                    dataRow["SessionID"] = sessionContextEntry.SessionID;
                    if (sessionContextEntry.OwnerClientGuid != null)
                    {
                        dataRow["OwnerClientGuid"] = sessionContextEntry.OwnerClientGuid;
                    }
                    dataRow["OwnerRemoteIPEndPoint"] = sessionContextEntry.OwnerRemoteIPEndPointToString;
                    dataRow["LastUpdateTime"] = sessionContextEntry.LastUpdateTime;
                    dataRow["LastClientHeartBeatTime"] = sessionContextEntry.LastClientHeartBeatTime;
                    dataRow["Presence"] = sessionContextEntry.Presence;
                }
                dataTable.Rows.Add
                        (
                            dataRow
                        );
            }
            DataAccess.UpdateSessions
                    (
                        dataTable
                    );
            
        }

        public static SingleThreadAsyncDequeueProcessor<MessageResponsedEntry> MessagesSyncToDataBaseAsyncQueueProcessor
            = new SingleThreadAsyncDequeueProcessor<MessageResponsedEntry>();

        private static DataTable _messagesEmptyDataTable =
                    new Func<DataTable>
                        (
                            () =>
                            {
                                var type = typeof(MessageResponsedEntry);
                                //new
                                //{
                                //    MessageID = 0L
                                //    ,
                                //    ReceiverAppID = "*"
                                //    ,
                                //    ReceiverGroupID = "*"
                                //    ,
                                //    ReceiverUserID = "*"
                                //    ,
                                //    SessionHostServer = ""
                                //    ,
                                //    ClientIPEndPoint = ""
                                //    ,
                                //    ResponsedTime = new Nullable<DateTime>()
                                //}.GetType();                //匿名类型 实例
                                var dataTable = DataTableHelper.GenerateEmptyDataTable(type, true);
                                return dataTable;
                            }
                        )();


        public static void RunMessagesSyncToDataBaseAsyncQueueProcess()
        {
            var dataTable = _messagesEmptyDataTable.Clone();

            MessagesSyncToDataBaseAsyncQueueProcessor
                .StartRunDequeuesThreadProcess
                    (
                        null
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .MessagesSyncAsyncToDataBaseWaitSleepInMilliseconds
                        , (x, y) =>
                        {
                            try
                            {
                                var list = y.Select
                                                (
                                                    (xx) =>
                                                    {
                                                        return xx.Item2; //MessageResponsedEntry
                                                    }
                                                );
                                MessagesSyncSaveToDataBaseProcess
                                    (
                                        dataTable
                                        , list
                                    );
                            }
                            finally
                            {
                                dataTable.Clear();
                            }

                          }
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .MessagesSyncAsyncToDataBaseWaitBatchTimeoutInSeconds * 1000
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .MessagesSyncAsyncToDataBaseWaitBatchMaxSizeInEntriesCount
                        , (x, y) =>
                        {
                            string s = string.Format
                                            (
                                                "{1}{0}{2}"
                                                , "\r\n"
                                                , y
                                                , x.ToString()
                                            );
                            Console.WriteLine(s);
                            EventLogHelper
                                .WriteEventLogEntry
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogSourceName
                                        , s
                                        , EventLogEntryType.Error
                                        , ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogDefaultErrorExceptionEventID
                                        , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionCategory
                                    );
                            return false;
                        }
                    );
        }

        private static void MessagesSyncSaveToDataBaseProcess(DataTable dataTable, IEnumerable<MessageResponsedEntry> list)
        {
            var query =
                        from
                            T in list
                        group
                            T by
                                new
                                {
                                    MessageID = T.MessageID
                                            ,
                                    ReceiverAppID = T.ReceiverAppID.ToLower().Trim()
                                            ,
                                    ReceiverGroupID = T.ReceiverGroupID.ToLower().Trim()
                                            ,
                                    ReceiverUserID = T.ReceiverUserID.ToLower().Trim()
                                }
                            into T1
                            //let 
                            //    SessionHostServer = T1.Max((x) => { return x.SessionHostServer; })
                            //let
                            //    ClientIPEndPoint = T1.Max((x) => { return x.ClientIPEndPoint; })
                            //let
                            //    FirstUdpPushTransmissionTime = T1.Min((x) => { return x.FirstUdpPushTransmissionTime; })
                            //let
                            //    LastUdpPushTransmissionTime = T1.Min((x) => { return x.LastUdpPushTransmissionTime; })
                            //let
                            //    UdpPushTransmissionTimes = T1.Min((x) => { return x.UdpPushTransmissionTimes; })
                            //let
                            //    UdpResponsedTime = T1.Min((x) => { return x.UdpResponsedTime; })
                            select
                            (
                                new
                                {
                                    MessageID = T1.Key.MessageID
                                    ,
                                    ReceiverAppID = T1.Key.ReceiverAppID.ToLower().Trim()
                                    ,
                                    ReceiverGroupID = T1.Key.ReceiverGroupID.ToLower().Trim()
                                    ,
                                    ReceiverUserID = T1.Key.ReceiverUserID.ToLower().Trim()
                                    ,
                                    SessionHostServer = T1.Max((x) => { return x.SessionHostServer; }).ToLower().Trim()
                                    ,
                                    ClientIPEndPoint = T1.Max((x) => { return x.ClientIPEndPoint; }).ToLower().Trim()
                                    ,
                                    FirstUdpPushTransmissionTime = T1.Min((x) => { return x.FirstUdpPushTransmissionTime; })
                                    ,
                                    LastUdpPushTransmissionTime = T1.Min((x) => { return x.LastUdpPushTransmissionTime; })
                                    ,
                                    UdpPushTransmissionTimes = T1.Min((x) => { return x.UdpPushTransmissionTimes; })
                                    ,
                                    UdpResponsedTime = T1.Min((x) => { return x.UdpResponsedTime; })
                                }
                            );
            foreach (var xx in query)
            {
                var dataRow = dataTable.NewRow();
                dataRow["MessageID"] = xx.MessageID;
                dataRow["ReceiverAppID"] = xx.ReceiverAppID;
                dataRow["ReceiverGroupID"] = xx.ReceiverGroupID;
                dataRow["ReceiverUserID"] = xx.ReceiverUserID;
                dataRow["SessionHostServer"] = xx.SessionHostServer;
                dataRow["ClientIPEndPoint"] = xx.ClientIPEndPoint;
                if (xx.FirstUdpPushTransmissionTime != null)
                {
                    dataRow["FirstUdpPushTransmissionTime"] = xx.FirstUdpPushTransmissionTime;
                }
                if (xx.LastUdpPushTransmissionTime != null)
                {
                    dataRow["LastUdpPushTransmissionTime"] = xx.LastUdpPushTransmissionTime;
                }
                dataRow["UdpPushTransmissionTimes"] = xx.UdpPushTransmissionTimes;
                if (xx.UdpResponsedTime != null)
                {
                    dataRow["UdpResponsedTime"] = xx.UdpResponsedTime;
                }
                dataTable.Rows.Add
                        (
                            dataRow
                        );
            }
            DataAccess.UpdateMessagesStatus
                        (
                            10          //已送达
                            , dataTable
                        );
        }
    }
}
#endif