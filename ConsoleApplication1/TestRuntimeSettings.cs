

namespace Test
{
    using Microshaoft;
    using System;
    public class TestRuntimeSettings
    {
        [ConfigurationAppSetting(SettingKey = "TimeStamp")]
        public DateTime MyTimeStamp
        {
            set;
            get;
        }
        [ConfigurationAppSetting(SettingKey = "Int")]
        public int MyInt
        {
            set;
            get;
        }
    }
}
