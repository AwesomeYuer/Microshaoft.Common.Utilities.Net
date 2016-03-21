namespace Test
{
    using Microshaoft;
    using System;
    public class TestRuntimeSettings
    {
        [ConfigurationAppSetting(SettingKey = "TimeStamp")]
        public DateTime? RuntimeTimeStamp
        {
            set;
            get;
        }
        [ConfigurationAppSetting(SettingKey = "Int")]
        public int RuntimeInt
        {
            set;
            get;
        }
    }
}
