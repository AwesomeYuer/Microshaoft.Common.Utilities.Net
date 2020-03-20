namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public static partial class DataReaderHelper
    {
        public static IEnumerable<TEntry> AsEnumerable<TEntry>
                (
                    this IDataReader @this
                    , bool needDefinitionAttributeProcess = false
                )
                    where TEntry : new()
        {
            return
                GetEnumerable<TEntry>
                    (
                        @this
                        , needDefinitionAttributeProcess
                    );
        }
        public static IEnumerable<TEntry> GetEnumerable<TEntry>
                (
                    this IDataReader @this
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
            while (@this.Read())
            {
                var entry = new TEntry();
                foreach (var x in members)
                {
                    var dataColumnName = x.Name;
                    if (needDefinitionAttributeProcess)
                    {
                        if (attribute != null)
                        {
                            if
                                (
                                    !attribute
                                        .DataTableColumnName
                                        .IsNullOrEmptyOrWhiteSpace()
                                )
                            {
                                dataColumnName = attribute.DataTableColumnName;
                            }
                        }
                    }
                    var setter = DynamicExpressionTreeHelper
                                        .CreateMemberSetter<TEntry, object>
                                            (
                                                x.Name
                                            );
                    setter
                            (
                                entry
                                , @this[dataColumnName]
                            );
                }
                yield
                    return
                        entry;
            }
        }
        
    }
}