namespace Test
{
    using Microshaoft;
    using System;
    //using System;
    using System.Linq.Expressions;


    class Program
    {
        static void Main(string[] args)
        {
            //Test Runtime Setting
            var appSettings = ConfigurationAppSettingsHelper
                                    .GetAppSettingsByMapFromConfig<TestRuntimeSettings>();


            var memberGetter = DynamicExpressionTreeHelper
                            .CreateMemberGetter<TestRuntimeSettings, object>
                                ("RuntimeTimeStamp");

           var  settingValue = memberGetter(appSettings);


           var dataTable =  DataTableHelper
                                    .GenerateEmptyDataTable<TestRuntimeSettings>(true);

            Console.ReadLine();
        }
    }
 class Foo
    {
        public int? Bar { get; set; }

        static void Main2()
        {
            var param = Expression.Parameter(typeof(Foo), "foo");
            Expression member = Expression.PropertyOrField(param, "Bar");


            Type typeIfNullable = Nullable.GetUnderlyingType(member.Type);
            if (typeIfNullable != null)
            {
                member = Expression.Call(member, "GetValueOrDefault", Type.EmptyTypes);
            }
            var body = Expression.Lambda<Func<Foo, int>>(member, param);

            var func = body.Compile();
            int result1 = func(new Foo { Bar = 123 }),
                result2 = func(new Foo { Bar = null });
        }
    }
}
