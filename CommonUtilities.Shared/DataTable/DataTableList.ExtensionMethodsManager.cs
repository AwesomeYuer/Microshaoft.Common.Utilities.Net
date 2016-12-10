
namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public static partial class DataTableListExtensionMethodsManager
    {
        public static DataRow[] FullTextSearch(this DataTable dataTable, string[] keyWords)
        {
            return
                dataTable
                    .AsEnumerable()
                    .Where<DataRow>
                        (
                            (x) =>
                            {
                                return
                                    keyWords
                                        .All
                                            (
                                                (xx) =>
                                                {
                                                    return
                                                        x
                                                            .ItemArray
                                                            .Select
                                                                (
                                                                    (xxx) =>
                                                                    {
                                                                        return xxx.ToString();
                                                                    }
                                                                )
                                                            .Any<string>
                                                                (
                                                                    (xxx) =>
                                                                    {
                                                                        return xxx.Contains(xx);
                                                                    }
                                                                );
                                                }
                                            );
                            }
                        ).ToArray();
        }
        public static DataTable ToDataTable<TEntry>(this IEnumerable<TEntry> target, bool needDefinitionAttributeProcess = false)
        {
            var type = typeof(TEntry);
            var accessors = TypeHelper.GenerateTypeKeyedCachedMembersAccessors(type, needDefinitionAttributeProcess);
            var dataTable = DataTableHelper
                                        .GenerateEmptyDataTable<TEntry>
                                                (needDefinitionAttributeProcess);
            var dataColumns = dataTable.Columns;
            if (dataTable != null)
            {
                foreach (var entry in target)
                {
                    var row = dataTable.NewRow();
                    foreach (DataColumn dataColumn in dataColumns)
                    {
                        MemberAccessor accessor = null;
                        if (accessors.TryGetValue(dataColumn.ColumnName, out accessor))
                        {
                            object v = accessor
                                            .Getter(entry);

                            if (v == null)
                            {
                                row[dataColumn] = DBNull.Value;
                            }
                            else
                            {
                                try
                                {
                                    row[dataColumn] = v;
                                }
                                catch
                                {
                                    row[dataColumn] = DBNull.Value;
                                }
                            }
                        }
                    }
                    dataTable
                            .Rows
                            .Add(row);
                }
            }
            return dataTable;
        }
        public static List<TEntry> ToList<TEntry>(this DataTable dataTable)
                                            where TEntry : new()
        {
            var type = typeof(TEntry);
            var columns = dataTable.Columns;
            var actions = new Dictionary<string, Action<object, object>>();
            foreach (DataColumn c in columns)
            {
                var columnName = c.ColumnName;
                var action = DynamicExpressionTreeHelper
                                    .CreateMemberSetter
                                                (
                                                    typeof(TEntry)
                                                    , columnName
                                                );
                actions[columnName] = action;
            }
            List<TEntry> list = null;
            var rows = dataTable.Rows;
            foreach (DataRow r in rows)
            {
                var entry = new TEntry();
                if (list == null)
                {
                    list = new List<TEntry>();
                }
                foreach (DataColumn c in columns)
                {
                    var columnName = c.ColumnName;
                    var v = r[columnName];
                    if
                        (
                            !DBNull
                                .Value
                                .Equals(v)
                        )
                    {
                        var action = actions[columnName];
                        action(entry, v);
                    }
                }
                list.Add(entry);
            }
            return list;
        }
    }
}
