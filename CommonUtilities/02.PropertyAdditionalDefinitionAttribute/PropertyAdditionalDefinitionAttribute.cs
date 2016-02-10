namespace Microshaoft
{
    using System;
    /// <summary>
    /// PropertyDefinitionAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class PropertyAdditionalDefinitionAttribute : Attribute
    {
        /// <summary>
        /// PropertyDefinitionAttribute 构造函数
        /// </summary>
        /// <param name="dataTableColumnID">列序号</param>
        public PropertyAdditionalDefinitionAttribute(int dataTableColumnID)
        {
            DataTableColumnID = dataTableColumnID;
        }
        public PropertyAdditionalDefinitionAttribute(int dataTableColumnID, string dataTableColumnName)
            : this(dataTableColumnID)
        {
            DataTableColumnName = dataTableColumnName;
        }
        public PropertyAdditionalDefinitionAttribute
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
            private set;
        }
        public string DataTableColumnName
        {
            get;
            private set;
        }
        public Type DataTableColumnDataType
        {
            get;
            private set;
        }
    }
}