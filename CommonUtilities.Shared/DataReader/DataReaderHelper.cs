
namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System;
    using Newtonsoft.Json.Linq;

    public static class DataReaderExtensionsMethodsManager
    {
        public static IEnumerable<T> ExecuteRead<T>
                (
                    this IDataReader target
                    , Func<int, IDataReader, T> onReadProcessFunc
                )
        {
            try
            {
                int i = 0;
                while (target.Read())
                {
                    if (onReadProcessFunc != null)
                    {
                        yield return
                            onReadProcessFunc(++i, target);
                    }
                }
            }
            finally
            {
                //可能有错 由于 yield 延迟
                target.Close();
                target.Dispose();
            }


        }
    }


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
                    , Func<IDataReader, TEntry> onReadProcessFunc
                    , bool skipNull = true
                )
                    where TEntry : new()
        {
            while (dataReader.Read())
            {
                var x = onReadProcessFunc(dataReader);
                if (!skipNull)
                {
                    yield
                        return
                                x;
                }
                else
                {
                    if (x != null)
                    {
                        yield
                           return
                                   x;
                    }
                }
            }


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
                    var setter = DynamicExpressionTreeHelper
                                            .CreateMemberSetter<TEntry, object>
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

        public static IEnumerable<JToken> AsJTokensEnumerable
                             (
                                 this IDataReader target
                             )
        {
            return
                GetJTokensEnumerable
                    (
                        target
                    );
        }

        public static IEnumerable<JToken> GetJTokensEnumerable
                             (
                                 IDataReader dataReader
                             )
        {
            var fieldsCount = dataReader.FieldCount;
            //SqlHelperParameterCache.GetSpParameterSet()
            while (dataReader.Read())
            {
                JObject row = new JObject();
                for (var i = 0; i < fieldsCount; i++)
                {
                    var fieldType = dataReader.GetFieldType(i);
                    var fieldName = dataReader.GetName(i);
                    JValue fieldValue = null;
                    
                    if
                        (
                            fieldType == typeof(bool)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetBoolean(i));
                    }
                    else if
                        (
                            fieldType == typeof(byte)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetByte(i));
                    }
                    else if
                        (
                            fieldType == typeof(char)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetChar(i));
                    }
                    else if
                        (
                            fieldType == typeof(DateTime)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetDateTime(i));
                    }
                    else if
                        (
                            fieldType == typeof(decimal)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetDecimal(i));
                    }
                    else if
                        (
                            fieldType == typeof(double)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetDouble(i));
                    }
                    else if
                        (
                            fieldType == typeof(float)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetFloat(i));
                    }
                    else if
                        (
                            fieldType == typeof(Guid)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetGuid(i));
                    }
                    else if
                        (
                            fieldType == typeof(short)
                        )
                    {
                        fieldValue = new JValue((long) dataReader.GetInt16(i));
                    }
                    else if
                        (
                            fieldType == typeof(int)
                        )
                    {
                        fieldValue = new JValue((long) dataReader.GetInt32(i));
                    }
                    else if
                        (
                            fieldType == typeof(long)
                        )
                    {
                        fieldValue = new JValue((long) dataReader.GetInt64(i));
                    }
                    else if
                        (
                            fieldType == typeof(string)
                        )
                    {
                        fieldValue = new JValue(dataReader.GetString(i));
                    }
                    else
                    {
                        fieldValue = new JValue(dataReader[i]);
                    }
                    var field = new JProperty(fieldName, fieldValue);
                    row.Add(field);
                }
                yield
                    return
                        row;
            }
        }
    }
}


