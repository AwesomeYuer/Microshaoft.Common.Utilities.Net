namespace Test
{
    using Microshaoft;
    using System;
    public class TestRuntimeSettings
    {
        [ConfigurationAppSetting(SettingKey = "TimeStamp")]
        public DateTime? RuntimeNullableTimeStamp;
        //{
        //    set;
        //    get;
        //}
        [ConfigurationAppSetting(SettingKey = "Int")]
        public int? RuntimeNullableInt
        {
            set;
            get;
        }

        [ConfigurationAppSetting(SettingKey = "Int")]
        public int? RuntimeInt;
        

        [ConfigurationAppSetting]
        public  DateTime? TimeStamp;

        [ConfigurationAppSetting]
        public  DateTime RuntimeTimeStamp;
    }
}
