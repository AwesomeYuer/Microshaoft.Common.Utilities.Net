namespace Microshaoft
{
    using System;
    /// <summary>
    /// PropertyDefinitionAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class MemberAdditionalDefinitionAttribute : Attribute
    {
        public MemberAdditionalDefinitionAttribute()
        {
            //DataTableColumnID = dataTableColumnID;
        }
        /// <summary>
        /// PropertyDefinitionAttribute 构造函数
        /// </summary>
        /// <param name="dataTableColumnID">列序号</param>
        public MemberAdditionalDefinitionAttribute(int dataTableColumnID)
        {
            DataTableColumnID = dataTableColumnID;
        }
        public MemberAdditionalDefinitionAttribute(int dataTableColumnID, string dataTableColumnName)
            : this(dataTableColumnID)
        {
            DataTableColumnName = dataTableColumnName;
        }
        public MemberAdditionalDefinitionAttribute
                        (
                            int dataTableColumnID
                            , string dataTableColumnName
                            , Type dataTableColumnDataType
                        )
            : this(dataTableColumnID, dataTableColumnName)
        {
            DataTableColumnDataType = dataTableColumnDataType;
        }
        public int? DataTableColumnID
        {
            get;
            set;
        }
        public string DataTableColumnName
        {
            get;
            set;
        }
        public Type DataTableColumnDataType
        {
            get;
            set;
        }
    }
}