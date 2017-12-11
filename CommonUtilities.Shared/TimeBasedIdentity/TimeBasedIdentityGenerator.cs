
namespace Microshaoft
{
    using System;
    using System.Threading;
    using System.Runtime.Serialization;
#if NETFRAMEWORK4_X
    using System.Web.Script.Serialization;
#endif
    using System.Xml.Serialization;
    using Newtonsoft.Json;

    public class TimeBasedIdentity //: ICloneable<>
    {
        //周期标识 按(周期容量个数)取模后第(余数)个周期

        public /*readonly*/ long PeriodID = -1;

        //周期标识 按(桶个数)取模后第(余数)个周期
        public /*readonly*/ long PeriodModID = -1;

        public /*readonly*/ DateTime? BasedDateTime;

        // 86400 秒
        public /*readonly*/ long PeriodInSeconds = -1;

        //1095 天
        public /*readonly*/ int PeriodsMaxCount = -1;

        //按秒对齐的时间
        public /*readonly*/ DateTime PeriodAlignedTime;

        //周期标识 按(桶个数)取模后第(余数)
#if NETFRAMEWORK4_X
        [ScriptIgnore]
#endif

        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public readonly int[] PeriodSeedsBucket = null;

#if NETFRAMEWORK4_X
        [ScriptIgnore]
#endif
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public readonly DateTime? InitializedTime;

#if NETFRAMEWORK4_X
        [ScriptIgnore]
#endif
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public readonly int InitializePeriodsCount = 0;

        public int ParsedSequenceIDInSecond = -1;

        public ulong ParsedSequenceID = 0;

        public int SecondID = -1;
        public DateTime? IdentityTime;

        public TimeBasedIdentity()
        {
        }

        public TimeBasedIdentity
                        (
                            long periodID
                            , long periodModID
                            , int[] periodSeedsBucket
                            , DateTime periodAlignedTime
                            , DateTime initializedTime
                            , DateTime basedDateTime
                            , long periodInSeconds
                            , int periodsMaxCount
                            , int initializePeriodsCount
                        )
        {
            PeriodID = periodID;
            PeriodModID = periodModID;
            PeriodSeedsBucket = periodSeedsBucket;
            PeriodAlignedTime = periodAlignedTime;
            InitializedTime = initializedTime;
            BasedDateTime = basedDateTime;
            PeriodInSeconds = periodInSeconds;
            PeriodsMaxCount = periodsMaxCount;
            InitializePeriodsCount = initializePeriodsCount;
        }


        public override string ToString()
        {
            return
                string
                    .Format
                        (
                            "{1}{2}{0}{3}{1}{4}{0}{5}{1}{6}{0}{7}{1}{8}{0}{9}{1}{10}{0}{11}{1}{12}{0}{13}{1}{14}{0}{15}{1}{16}{0}{17}{1}{18}{0}{19}{1}{20}{0}{21}{1}{22}{0}{23}{1}{24}{0}{25}{1}"

                            , " : "
                            , "\r\n"

                            , "PeriodID"
                            , PeriodID

                            , "PeriodModID"
                            , PeriodModID

                            , "PeriodSeedsBucket"
                            ,
                                (
                                    PeriodSeedsBucket != null
                                    ?
                                    -1 : PeriodSeedsBucket.Length
                                )

                            , "SecondID"
                            , SecondID

                            , "CurrentSeedInCurrentBucket"
                            , PeriodSeedsBucket[SecondID]
                            ,
                                (
                                    PeriodSeedsBucket != null
                                    ?
                                    -1 : PeriodSeedsBucket[SecondID]
                                )


                            , "IdentityTime"
                            , IdentityTime

                            , "PeriodAlignedTime"
                            , PeriodAlignedTime

                            , "InitializedTime"
                            , InitializedTime

                            , "BasedTime"
                            , BasedDateTime


                            , "PeriodInSeconds"
                            , PeriodInSeconds

                            , "PeriodsMaxCount"
                            , PeriodsMaxCount

                            , "InitializePeriodsCount"
                            , InitializePeriodsCount


                        );
        }


        public ulong GetNewSequenceID
                        (
                            int lowestDecimalDigitsCount = 4
                        )
        {
            ulong r = (ulong)SecondID;
            if (r >= 0)
            {
                var i = Interlocked.Increment(ref PeriodSeedsBucket[SecondID]);
                r = GetSequenceID(i, lowestDecimalDigitsCount);
            }
            return r;
        }


        public ulong GetSequenceID
                (
                    int sequenceIDInPeroid
                    , int lowestDecimalDigitsCount = 4
                )
        {
            ulong r = (ulong)SecondID;
            if (r >= 0)
            {
                //r = 1;
                //r *= factor;
                var s = string.Empty;
                var ss = PeriodsMaxCount.ToString();
                var l = ss.Length;
                ss = PeriodModID.ToString();
                s += ss.PadLeft(l, '0');

                ss = PeriodInSeconds.ToString();
                l = ss.Length;
                ss = SecondID.ToString();
                s += ss.PadLeft(l, '0');

                //var i = Interlocked.Increment(ref PeriodSeedsBucket[SecondID]);
                l = lowestDecimalDigitsCount;
                ss = sequenceIDInPeroid.ToString();
                s += ss.PadLeft(l, '0');

                r = ulong.Parse(s);

                //long x = SecondID;
                ////for (var i = 0; i < l; i++)
                //x += (long) BigInteger.Pow(10, l - 1);
                //r += x;
                ////========================================
                //l += PeriodsMaxCount.ToString().Length;
                //x = PeriodModID;
                //x += (long)BigInteger.Pow(10, l - 1);
                //r += x;
                //=========================================

            }
            return r;
        }


        public TimeBasedIdentity PartialClone()
        {
            return
                new TimeBasedIdentity
                        (
                            PeriodID
                            , PeriodModID
                            , PeriodSeedsBucket
                            , PeriodAlignedTime
                            , InitializedTime.Value
                            , BasedDateTime.Value
                            , PeriodInSeconds
                            , PeriodsMaxCount
                            , InitializePeriodsCount
                        );
        }
    }
}
namespace Microshaoft
{
    using System;

    public class TimeBasedIdentityGenerator
    {

        public readonly TimeBasedIdentity[]
                                PeriodsSeedsBuckets = null;
        private DateTime _basedDateTime = default(DateTime);
        private long _periodInSeconds = 0;
        private int _periodsMaxCount = 0;
        private int _initializePeriodsCount = 0;
        public TimeBasedIdentityGenerator
                    (
                        long periodInSeconds = 86400
                        , int periodsMaxCount = 10000
                        , int initializeMinPeriodsBucketsCount = 2
                        , Action<TimeBasedIdentityGenerator> onAfterDataInitializedProcessAction = null
                        , DateTime basedDateTime = default(DateTime)
                        , int checkTimes = 2
                    )
        {

            _basedDateTime = basedDateTime;
            _periodInSeconds = periodInSeconds;
            _periodsMaxCount = periodsMaxCount;
            var remainder = _periodsMaxCount % initializeMinPeriodsBucketsCount;
            if (remainder != 0)
            {
                while (_periodsMaxCount % (++initializeMinPeriodsBucketsCount) != 0) ;
            }
            //桶个数
            _initializePeriodsCount = initializeMinPeriodsBucketsCount;
            var diffSeconds = DateTimeHelper
                                    .SecondsDiff
                                        (_basedDateTime, DateTime.Now);
            PeriodsSeedsBuckets = new TimeBasedIdentity[_initializePeriodsCount];
            var l = PeriodsSeedsBuckets.Length;
            long periodID = (diffSeconds / _periodInSeconds);
            for (int i = 0; i < l; i++)
            {
                var periodModID = periodID % _periodsMaxCount;
                var alignedTime = _basedDateTime
                                    .AddSeconds
                                        (periodID * _periodInSeconds);
                PeriodsSeedsBuckets[periodModID % l]
                                        = new TimeBasedIdentity
                                                    (
                                                        periodID
                                                        , periodModID
                                                        , new int[_periodInSeconds]
                                                        , alignedTime
                                                        , DateTime.Now
                                                        , _basedDateTime
                                                        , _periodInSeconds
                                                        , _periodsMaxCount
                                                        , _initializePeriodsCount
                                                    );
                periodID++;
                //periodModID++;
                //periodModID = periodModID % _periodsMaxCount;
            }
            if (onAfterDataInitializedProcessAction != null)
            {
                onAfterDataInitializedProcessAction(this);
            }
            int timerIntervalInSeconds = (int)_periodInSeconds / checkTimes;
            new EasyTimer
                    (
                        timerIntervalInSeconds
                        , 2
                        , (x) =>
                        {
                            if (InitializeNextOnePeriodDataProcessAction())
                            {
                                if (onAfterDataInitializedProcessAction != null)
                                {
                                    onAfterDataInitializedProcessAction(this);
                                }
                            }
                        }
                    ).Start();
        }
        public TimeBasedIdentity GetNewID(DateTime time)
        {
            long diffSeconds = DateTimeHelper
                                    .SecondsDiff
                                        (_basedDateTime, time);
            long periodID = (diffSeconds / _periodInSeconds);
            long periodModID = periodID % _periodsMaxCount;
            int secondID = (int)(diffSeconds % _periodInSeconds);
            int l = PeriodsSeedsBuckets.Length;
            long bucketID = periodModID % l;
            TimeBasedIdentity timeBasedIdentity = PeriodsSeedsBuckets[bucketID];
            //timeBasedIdentity.SecondID = secondID;
            TimeBasedIdentity r = null;
            if
                (
                    timeBasedIdentity.PeriodModID == periodModID
                )
            {
                r = timeBasedIdentity.PartialClone();
                r.SecondID = secondID;
                r.IdentityTime = r.PeriodAlignedTime.AddSeconds(secondID);
            }
            return r;
        }
        public TimeBasedIdentity GetNewID()
        {
            return GetNewID(DateTime.Now);
        }
        private bool InitializeNextOnePeriodDataProcessAction()
        {
            var r = false;
            var now = DateTime.Now;
            //now = DateTime.Parse("9999-12-31 23:10:00");
            //周期标识
            var diffSeconds = DateTimeHelper
                                    .SecondsDiff(_basedDateTime, now);
            long periodID = (diffSeconds / _periodInSeconds);
            var alignedTime = _basedDateTime
                                    .AddSeconds
                                        (periodID * _periodInSeconds);
            periodID++;
            var periodModID = periodID % _periodsMaxCount;
            int l = PeriodsSeedsBuckets.Length;
            long bucketID = periodID % l;
            var lastPeriodID = PeriodsSeedsBuckets[bucketID].PeriodID;
            var lastPeriodModID = PeriodsSeedsBuckets[bucketID].PeriodModID;
            if
                (
                    periodID > lastPeriodID
                    &&
                    periodModID != lastPeriodModID
                )
            {
                PeriodsSeedsBuckets[bucketID]
                                = new TimeBasedIdentity
                                        (
                                            periodID
                                            , periodModID
                                            , new int[_periodInSeconds]
                                            , alignedTime
                                            , DateTime.Now
                                            , _basedDateTime
                                            , _periodInSeconds
                                            , _periodsMaxCount
                                            , _initializePeriodsCount
                                        );
                r = true;
            }
            return r;
        }
    }
}

