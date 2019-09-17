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
        [
            MemberAdditionalDefinition
                (
                    10
                    ,
                    DataTableColumnDataType = typeof(int)
                    ,  DataTableColumnName = "ColumnRuntimeNullableInt"
                    
                )
        ]
        public int? RuntimeNullableInt
        {
            set;
            get;
        }

        [ConfigurationAppSetting(SettingKey = "Int")]

        public int RuntimeInt = -1;
        

        [ConfigurationAppSetting]
        [
            MemberAdditionalDefinition
                (
                    12
                    ,
                    DataTableColumnDataType = typeof(int)
                    , DataTableColumnName = "ColumnTimeStamp"

                )
        ]
        public  DateTime? TimeStamp;

        [ConfigurationAppSetting]
        public  DateTime RuntimeTimeStamp;


        [ConfigurationAppSetting]
        public string RuntimeTimeStampS;

        [ConfigurationAppSetting]
        public Guid GuidV;

        [ConfigurationAppSetting]
        public Guid? GuidNullable;

    }
}
