namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    public static class DataReaderHelper
    {
        public static IEnumerable<TEntry> AsEnumerable<TEntry>
                        (
                            this SqlDataReader target
                            , bool needDefinitionAttributeProcess = false
                        )
                            where TEntry : new()
        {
            return

                    GetEnumerable<TEntry>
                        (
                            target
                            , needDefinitionAttributeProcess
                        );
        }

        public static IEnumerable<TEntry> GetEnumerable<TEntry>
                        (
                            IDataReader dataReader
                            , bool needDefinitionAttributeProcess = false
                        )
                            where TEntry : new()
        {
            var type = typeof(TEntry);
            var accessors = TypeHelper
                                .GetTypePropertiesAccessors
                                        (
                                            type
                                            , needDefinitionAttributeProcess
                                        );
            while (dataReader.Read())
            {
                TEntry entry = new TEntry();
                foreach (var accessor in accessors)
                {
                    var propertyName = accessor.PropertyName;
                    var columnName = propertyName;
                    if (needDefinitionAttributeProcess)
                    {
                        columnName = accessor
                                        .DefinitionAttribute
                                        .DataTableColumnName;
                    }
                    accessor
                        .Setter
                            (
                                entry
                                , dataReader[columnName]
                            );
                }
                yield
                    return
                        entry;
            }
        }
    }
}
