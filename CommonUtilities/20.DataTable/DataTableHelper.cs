namespace Microshaoft
{
    using System;
    using System.Data;
    using System.Linq;

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
            var accessors = TypeHelper
                                .GetTypePropertiesAccessors
                                        (
                                            type
                                            , needDefinitionAttributeProcess
                                        );
            if (needDefinitionAttributeProcess)
            {
                accessors = accessors
                                .OrderBy
                                    (
                                        (x) =>
                                        {
                                            return
                                                    x
                                                        .DefinitionAttribute
                                                        .DataTableColumnID;
                                        }
                                    );
            }
            DataTable dataTable = null;
            DataColumnCollection dataColumnsCollection = null;
            foreach (var x in accessors)
            {
                if (dataTable == null)
                {
                    dataTable = new DataTable();
                }
                if (dataColumnsCollection == null)
                {
                    dataColumnsCollection = dataTable.Columns;
                }
                dataColumnsCollection
                    .Add
                        (
                            x.PropertyKey
                            , x.PropertyValueType
                        );
            }
            return dataTable;
        }
        public static void DataTableRowsForEach
                                (
                                    DataTable dataTable
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
                dataColumnCollection = dataTable.Columns;
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
                    dataColumnCollection = dataTable.Columns;
                }
                r = processHeaderDataColumnsFunc(dataColumnCollection);
                if (r)
                {
                    return;
                }
            }
            DataRowCollection drc = null;
            if
                (
                    processRowDataColumnFunc != null
                    || processRowFunc != null
                )
            {
                drc = dataTable.Rows;
                if
                    (
                        (
                            processRowDataColumnFunc != null
                            || processRowFunc != null
                        )
                        && dataColumnCollection == null
                    )
                {
                    dataColumnCollection = dataTable.Columns;
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
                    if (processRowFunc != null)
                    {
                        processRowFunc(dataColumnCollection, dataRow, i);
                    }
                }
            }
        }
    }
}
