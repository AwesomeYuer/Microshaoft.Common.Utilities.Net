#if NETCOREAPP3_X || NETSTANDARY2_X
namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    public static class ValueTupleHelper
    {
        public static DataTable GenerateEmptyDataTable
                                    <T1>
                (
                    this ValueTuple<T1> target
                    , string[] dataColumnsNames
                )
        {
            return
                GenerateEmptyDataTable
                        (
                            target.GetType()
                            , dataColumnsNames
                        );
        }
        public static DataTable GenerateEmptyDataTable
                                    <T1, T2>
            (
                this ValueTuple<T1, T2> target
                , params string[] dataColumnsNames
            )
        {
            return
                GenerateEmptyDataTable
                        (
                            target.GetType()
                            , dataColumnsNames
                        );
        }
        public static DataTable GenerateEmptyDataTable
                                    <T1, T2, T3>
                (
                    this ValueTuple<T1, T2, T3> target
                    , params string[] dataColumnsNames
                )
        {
            return
                GenerateEmptyDataTable
                        (
                            target.GetType()
                            , dataColumnsNames
                        );
        }
        public static DataTable GenerateEmptyDataTable
                                    <T1, T2, T3, T4>
                (
                    this ValueTuple<T1, T2, T3, T4> target
                    , params string[] dataColumnsNames
                )
        {
            return
                GenerateEmptyDataTable
                        (
                            target.GetType()
                            , dataColumnsNames
                        );
        }
        public static DataTable GenerateEmptyDataTable
                                    <T1, T2, T3, T4, T5>
                (
                    this ValueTuple<T1, T2, T3, T4, T5> target
                    , params string[] dataColumnsNames
                )
        {
            return
                GenerateEmptyDataTable
                        (
                            target.GetType()
                            , dataColumnsNames
                        );
        }
        public static DataTable GenerateEmptyDataTable
                                    <T1, T2, T3, T4, T5, T6>
                (
                    this ValueTuple<T1, T2, T3, T4, T5, T6> target
                    , params string[] dataColumnsNames
                )
        {
            return
                GenerateEmptyDataTable
                        (
                            target.GetType()
                            , dataColumnsNames
                        );
        }
        public static DataTable GenerateEmptyDataTable
                                    <T1, T2, T3, T4, T5, T6, T7>
                (
                    this ValueTuple<T1, T2, T3, T4, T5, T6, T7> target
                    , params string[] dataColumnsNames
                )
        {
            return
                GenerateEmptyDataTable
                        (
                            target.GetType()
                            , dataColumnsNames
                        );
        }

        private static IEnumerable<DataColumn> GenerateDataColumns
                (
                    Type valueTupleType
                    , string prefixPath
                    , string duplicateNameSuffix = null
                    , bool checkIsValueTupleType = true
                    , HashSet<string> distinctedNames = null
                    , params string[] dataColumnsNames
                )
        {
            if 
                (
                    checkIsValueTupleType
                    &&
                    !valueTupleType.IsValueTupleType()
                )
            {
                throw
                    new Exception($"Type: {valueTupleType.Name} is not ValueTuple Type");
            }
            var fields = valueTupleType.GetFields();
            var i = 0;
            var l = 0;
            if (distinctedNames == null)
            {
                distinctedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            if (dataColumnsNames != null)
            {
                l = dataColumnsNames.Length;
            }
            foreach (var field in fields)
            {
                var isDrilled = false;
                var dataColumnName = field.Name;
                //dataColumnName = valueTupleType.GetRuntimeField($"Item{i+1}").Name;
                var dataColumnType = field.FieldType;
                if (dataColumnType.IsNullableType())
                {
                    dataColumnType = dataColumnType
                                            .GetNullableUnderlyingType();
                }
                if (dataColumnType.IsValueTupleType())
                {
                    var entries = GenerateDataColumns
                                (
                                    dataColumnType
                                    , $@"{prefixPath}\{field.Name}"
                                    , duplicateNameSuffix
                                    , false
                                    , distinctedNames
                                    ,
                                        (
                                            (i < l)
                                            ?
                                            dataColumnsNames[i..]
                                            :
                                            null
                                        )
                                    );
                    foreach (var entry in entries)
                    {
                        yield
                            return
                                entry;
                    }
                    isDrilled = true;
                }
                else
                {
                    if (i < l)
                    {
                        dataColumnName = dataColumnsNames[i];
                        while
                            (
                                distinctedNames
                                            .Contains
                                                (
                                                    dataColumnName
                                                    , StringComparer
                                                            .OrdinalIgnoreCase
                                                )
                            )
                        {
                            dataColumnName = $"{dataColumnName}{duplicateNameSuffix}";
                        }
                        distinctedNames
                                    .Add
                                        (
                                            dataColumnName
                                        );
                    }
                }
                if (!isDrilled)
                {
                    yield
                        return
                            new DataColumn
                            {
                                ColumnName = dataColumnName
                                , DataType = dataColumnType
                            };
                    i++;
                }
                
            }
            distinctedNames = null;
        }
        public static IEnumerable<DataColumn> GenerateDataColumns
                (
                    this Type valueTupleType
                    , params string[] dataColumnsNames
                )
        {
            return
                GenerateDataColumns
                        (
                            valueTupleType
                            , default
                            , default
                            , true
                            , null
                            , dataColumnsNames
                        );
        }
        public static IEnumerable<DataColumn> GenerateDataColumns
        (
            this Type valueTupleType
            , string duplicateNameSuffix = null
            , params string[] dataColumnsNames
        )
        {
            return
                GenerateDataColumns
                        (
                            valueTupleType
                            , duplicateNameSuffix
                            , null
                            , true
                            , null
                            , dataColumnsNames
                        );
        }

        public static DataTable GenerateEmptyDataTable
                (
                    this Type valueTupleType
                    , params string[] dataColumnsNames
                )
        {
            if (!valueTupleType.IsValueTupleType())
            {
                throw
                    new Exception($"Type: {valueTupleType.Name} is not ValueTuple Type");
            }
            DataTable dataTable = null;
            var fields = valueTupleType.GetFields();
            DataColumnCollection dataColumnsCollection = null;
            if (dataColumnsNames != null)
            {
                var l = dataColumnsNames.Length;
                var i = 0;
                foreach (var field in fields)
                {
                    if (dataTable == null)
                    {
                        dataTable = new DataTable();
                        if (dataColumnsCollection == null)
                        {
                            dataColumnsCollection = dataTable.Columns;
                        }
                    }
                    var dataColumnName = field.Name;
                    //dataColumnName = valueTupleType.GetRuntimeField($"Item{i+1}").Name;
                    if (i < l)
                    {
                        dataColumnName = dataColumnsNames[i];
                    }
                    var dataColumnType = field.FieldType;
                    if (dataColumnType.IsNullableType())
                    {
                        dataColumnType = dataColumnType
                                                .GetNullableUnderlyingType();
                    }
                    dataColumnsCollection
                                        .Add
                                            (
                                                dataColumnName
                                                , dataColumnType
                                            );
                    i ++;
                }
            }
            return dataTable;
        }

        private static readonly
                HashSet<Type>
                    _valueTupleTypes = new HashSet<Type>
                        (
                            new Type[]
                            {
                                typeof(ValueTuple<>),
                                typeof(ValueTuple<,>),
                                typeof(ValueTuple<,,>),
                                typeof(ValueTuple<,,,>),
                                typeof(ValueTuple<,,,,>),
                                typeof(ValueTuple<,,,,,>),
                                typeof(ValueTuple<,,,,,,>),
                                typeof(ValueTuple<,,,,,,,>)
                            }
                        );

        public static bool
                IsValueTuple(this object target) => IsValueTupleType(target.GetType());
        public static bool IsValueTupleType(this Type target)
        {
            return
                (
                    target
                        .GetTypeInfo()
                        .IsGenericType
                    &&
                    _valueTupleTypes
                        .Contains
                            (
                                target
                                    .GetGenericTypeDefinition()
                            )
                );
        }

        public static
                List<object>
                        GetValueTupleItemObjects
                            (this object target)
                            =>
                            GetValueTupleItemFields
                                            (target.GetType())
                                    .Select(f => f.GetValue(target))
                                    .ToList();
        public static
                List<Type>
                        GetValueTupleItemTypes(this Type target)
                            =>
                            GetValueTupleItemFields(target)
                                .Select(f => f.FieldType)
                                .ToList();
        public static List<FieldInfo> GetValueTupleItemFields(this Type target)
        {
            var runtimeFields = new List<FieldInfo>();

            FieldInfo runtimeField;
            int nth = 1;
            while
                (
                    (runtimeField = target.GetRuntimeField($"Item{nth}"))
                    !=
                    null
                )
            {
                nth++;
                runtimeFields.Add(runtimeField);
            }
            return runtimeFields;
        }

        public static DataRow Add
                        <
                              T1
                            //, T2
                            //, T3
                            //, T4
                            //, T5
                            //, T6
                            //, T7
                        >
                    (
                        this DataRowCollection target
                        , ValueTuple
                                    <
                                          T1
                                        //, T2
                                        //, T3
                                        //, T4
                                        //, T5
                                        //, T6
                                        //, T7
                                    >
                                        rowData
                    )
        {
            return
                target
                    .Add
                        (
                              rowData.Item1
                            //, rowData.Item2
                            //, rowData.Item3
                            //, rowData.Item4
                            //, rowData.Item5
                            //, rowData.Item6
                        );
        }
        public static DataRow Add
                                <
                                      T1
                                    , T2
                                    //, T3
                                    //, T4
                                    //, T5
                                    //, T6
                                    //, T7
                                >
                            (
                                this DataRowCollection target
                                , ValueTuple
                                            <
                                                  T1
                                                , T2
                                                //, T3
                                                //, T4
                                                //, T5
                                                //, T6
                                                //, T7
                                            >
                                                rowData
                            )
        {
            return
                target
                    .Add
                        (
                              rowData.Item1
                            , rowData.Item2
                            //, rowData.Item3
                            //, rowData.Item4
                            //, rowData.Item5
                            //, rowData.Item6
                        );
        }

        public static DataRow Add
                        <
                              T1
                            , T2
                            , T3
                            //, T4
                            //, T5
                            //, T6
                            //, T7
                        >
                    (
                        this DataRowCollection target
                        , ValueTuple
                                    <
                                          T1
                                        , T2
                                        , T3
                                        //, T4
                                        //, T5
                                        //, T6
                                        //, T7
                                    >
                                        rowData
                    )
        {
            return
                target
                    .Add
                        (
                              rowData.Item1
                            , rowData.Item2
                            , rowData.Item3
                            //, rowData.Item4
                            //, rowData.Item5
                            //, rowData.Item6
                        );
        }

        public static DataRow Add
                <
                      T1
                    , T2
                    , T3
                    , T4
                    //, T5
                    //, T6
                    //, T7
                >
            (
                this DataRowCollection target
                , ValueTuple
                            <
                                  T1
                                , T2
                                , T3
                                , T4
                                //, T5
                                //, T6
                                //, T7
                            >
                                rowData
            )
        {
            return
                target
                    .Add
                        (
                              rowData.Item1
                            , rowData.Item2
                            , rowData.Item3
                            , rowData.Item4
                            //, rowData.Item5
                            //, rowData.Item6
                        );
        }
        public static DataRow Add
                <
                      T1
                    , T2
                    , T3
                    , T4
                    , T5
                    //, T6
                    //, T7
                >
            (
                this DataRowCollection target
                , ValueTuple
                            <
                                  T1
                                , T2
                                , T3
                                , T4
                                , T5
                                //, T6
                                //, T7
                            >
                                rowData
            )
        {
            return
                target
                    .Add
                        (
                              rowData.Item1
                            , rowData.Item2
                            , rowData.Item3
                            , rowData.Item4
                            , rowData.Item5
                            //, rowData.Item6
                        );
        }
        public static DataRow Add
                <
                      T1
                    , T2
                    , T3
                    , T4
                    , T5
                    , T6
                    //, T7
                >
            (
                this DataRowCollection target
                , ValueTuple
                            <
                                  T1
                                , T2
                                , T3
                                , T4
                                , T5
                                , T6
                                //, T7
                            >
                                rowData
            )
        {
            return
                target
                    .Add
                        (
                              rowData.Item1
                            , rowData.Item2
                            , rowData.Item3
                            , rowData.Item4
                            , rowData.Item5
                            , rowData.Item6
                        );
        }
    }
}



namespace ConsoleApp57
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Microshaoft;
    public static class Program
    {
        static void Main(string[] args)
        {

            //var (string F1, int F2, DateTime F3)

           (string F1, int F2, DateTime F3)
            x = ("asdsad", 100, DateTime.Now);

            var vt1 = (name: "asdsa", age: 13, Birthday: DateTime.Now);
            var vt2 = (name1: "asdsa", age1: 13, Birthday1: DateTime.Now);
            DataTable dataTable = x.GenerateEmptyDataTable(nameof(x.F1), nameof(x.F2), "F222");
            //var dataTable = 
            dataTable.Rows.Add(x.F1, x.F2, x.F3);
            dataTable.Rows.Add(vt1);
            dataTable.Rows.Add(vt2);
            //var F11 = "asdasd";
            //var F22 = 100;
            //var F33 = DateTime.Now;

            //var (string a, int b, DateTime c) =


            SqlConnection sqlConnection = new SqlConnection
                (
                    "Initial Catalog=test;Data Source=gateway.hyper-v.internal\\sql2019,11433;User=sa;Password=!@#123QWE"
                );




            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = "usp_testudt";
            sqlCommand.CommandType = CommandType.StoredProcedure;
            var sqlParameter = new SqlParameter("a", SqlDbType.Structured);
            sqlParameter.Value = dataTable;
            sqlCommand.Connection = sqlConnection;
            sqlCommand.Parameters.Add(sqlParameter);
            sqlConnection.Open();
            var r = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            while (r.Read())
            {
                Console.WriteLine(r.FieldCount);

            }



            Console.WriteLine("Hello World!");
        }


    }
}
#endif