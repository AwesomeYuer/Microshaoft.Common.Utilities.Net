namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public static partial class DataReaderHelper
    {
        public static IEnumerable<TEntry> AsEnumerable<TEntry>
                (
                    this IDataReader target
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
                    this IDataReader target
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
            while (target.Read())
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
                                , target[dataColumnName]
                            );
                }
                yield
                    return
                        entry;
            }
        }
        
    }
}