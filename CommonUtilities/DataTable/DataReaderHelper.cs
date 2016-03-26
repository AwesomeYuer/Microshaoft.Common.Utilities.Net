namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
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
            MemberAdditionalDefinitionAttribute attribute = null;
            var members = TypeHelper
                                .GetModelMembers(type)
                                .Select
                                    (
                                        (x) =>
                                        {
                                            attribute = 
                                                        x
                                                            .GetCustomAttributes
                                                                (typeof(MemberAdditionalDefinitionAttribute), true)
                                                            .FirstOrDefault() as MemberAdditionalDefinitionAttribute;
                                            return x;
                                        }
                                    );
            while (dataReader.Read())
            {
                TEntry entry = new TEntry();
                foreach (var x in members)
                {
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
                    var setter = DynamicMemberAccessor
                                            .CreateSetter<TEntry, object>
                                                (
                                                    x.Name
                                                );
                    setter
                            (
                                entry
                                , dataReader[dataColumnName]
                            );
                }
                yield
                    return
                        entry;
            }
        }
    }
}
