namespace Microshaoft
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    public static class DataTableHelper
    {
        public static DataTable GenerateEmptyDataTable<T>(bool needDefinitionAttributeProcess = false)
        {
            var type = typeof(T);
            return GenerateEmptyDataTable(type, needDefinitionAttributeProcess);
        }
        public static DataTable GenerateEmptyDataTable
                (
                    Type type
                    , bool needDefinitionAttributeProcess = false
                )
        {
            MemberAdditionalDefinitionAttribute attribute = null;
            var members = TypeHelper
                                .GetModelMembers(type)
                                .OrderBy
                                    (
                                        (x) =>
                                        {
                                            int r = int.MinValue;
                                            attribute = x
                                                            .GetCustomAttributes
                                                                (typeof(MemberAdditionalDefinitionAttribute), true)
                                                            .FirstOrDefault() as MemberAdditionalDefinitionAttribute;
                                            if (attribute != null)
                                            {
                                                if (attribute.DataTableColumnID.HasValue)
                                                {
                                                    r = attribute.DataTableColumnID.Value;
                                                }
                                            }
                                            return r;
                                        }
                                    );
            DataTable dataTable = null;
            DataColumnCollection dataColumnsCollection = null;
            foreach (var x in members)
            {
                if (dataTable == null)
                {
                    dataTable = new DataTable();
                    if (dataColumnsCollection == null)
                    {
                        dataColumnsCollection = dataTable.Columns;
                    }
                }
                var dataColumnName = x.Name;
                if (needDefinitionAttributeProcess)
                {
                    if (attribute != null)
                    {
                        if (!attribute.DataTableColumnName.IsNullOrEmptyOrWhiteSpace())
                        {
                            dataColumnName = attribute.DataTableColumnName;
                        }
                    }
                }
                Type dataColumnType = null;
                if (x is FieldInfo)
                {
                    var fieldInfo = x as FieldInfo;
                    dataColumnType = fieldInfo.FieldType;
                }
                else if (x is PropertyInfo)
                {
                    var propertyInfo = x as PropertyInfo;
                    dataColumnType = propertyInfo.PropertyType;
                }
                if (needDefinitionAttributeProcess)
                {
                    if (attribute != null)
                    {
                        if (attribute.DataTableColumnDataType != null)
                        {
                            dataColumnType = attribute.DataTableColumnDataType;
                        }
                    }
                }
                if (dataColumnType.IsNullableType())
                {
                    dataColumnType = TypeHelper.GetNullableUnderlyingType(dataColumnType);

                }
                dataColumnsCollection
                    .Add
                        (
                            dataColumnName
                            , dataColumnType
                        );

            }
            return dataTable;
        }
        public static void RowsForEach
                                (
                                    this DataTable target
                                    , Func<DataColumn, int, bool>
                                            processHeaderDataColumnFunc = null
                                    , Func<DataColumnCollection, bool>
                                            processHeaderDataColumnsFunc = null
                                    , Func<DataColumn, int, object, int, bool>
                                            processRowDataColumnFunc = null
                                    , Func<DataColumnCollection, DataRow, int, bool>
                                            processRowFunc = null
                                )
        {
            DataColumnCollection dataColumnCollection = null;
            int i = 0;
            bool r = false;
            if (processHeaderDataColumnFunc != null)
            {
                dataColumnCollection = target.Columns;
                foreach (DataColumn dc in dataColumnCollection)
                {
                    i++;
                    r = processHeaderDataColumnFunc(dc, i);
                    if (r)
                    {
                        break;
                    }
                }
            }
            if (processHeaderDataColumnsFunc != null)
            {
                if (dataColumnCollection == null)
                {
                    dataColumnCollection = target.Columns;
                }
                r = processHeaderDataColumnsFunc(dataColumnCollection);
                if (r)
                {
                    return;
                }
            }

            if
                (
                    processRowDataColumnFunc != null
                    ||
                    processRowFunc != null
                )
            {
                DataRowCollection drc = target.Rows;
                if
                    (
                        (
                            processRowDataColumnFunc != null
                            || processRowFunc != null
                        )
                        && dataColumnCollection == null
                    )
                {
                    dataColumnCollection = target.Columns;
                }
                i = 0;
                var j = 0;
                foreach (DataRow dataRow in drc)
                {
                    i++;
                    foreach (DataColumn dc in dataColumnCollection)
                    {
                        if (processRowDataColumnFunc != null)
                        {
                            j++;
                            r = processRowDataColumnFunc
                                    (
                                        dc
                                        , j
                                        , dataRow[dc]
                                        , i
                                    );
                            if (r)
                            {
                                j = 0;
                                break;
                            }
                        }
                    }
                    processRowFunc?
                                .Invoke
                                    (
                                        dataColumnCollection
                                        , dataRow
                                        , i
                                    );
                }
            }
        }
    }
}
