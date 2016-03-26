namespace Microshaoft
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Reflection;
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class ConfigurationAppSettingAttribute : Attribute
    {
        public string SettingKey;
    }

    public static class ConfigurationAppSettingsHelper
    {
        public static T GetAppSettingsFormConfig<T>() where T : new()
        {
            T r = new T();
            Type type = typeof(T);
            string settingKey = string.Empty;
            var members = type
                            .GetCustomAttributedPropertiesOrFields
                                    <ConfigurationAppSettingAttribute>
                                        (
                                            (x, y, z) =>
                                            {
                                                settingKey = z.SettingKey;
                                                if (settingKey.IsNullOrEmptyOrWhiteSpace())
                                                {
                                                    settingKey = y.Name;
                                                }
                                                return true;
                                            }
                                        );
            foreach (var member in members)
            {
                var settingValueText = ConfigurationManager.AppSettings[settingKey];
                Type memberValueType = null;
                if (member.MemberType == MemberTypes.Field)
                {
                    var fieldInfo = (FieldInfo)member;
                    memberValueType = fieldInfo.FieldType;
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var propertyInfo = (PropertyInfo)member;
                    memberValueType = propertyInfo.PropertyType;
                }
                if (memberValueType.IsNullableType())
                {
                    memberValueType = memberValueType
                                        .GetNullableUnderlyingType();
                }
                var methodInfo = memberValueType
                                        .GetMethod
                                            (
                                                "Parse"
                                                , new Type[] { typeof(string) }
                                            );
                if (methodInfo != null)
                {
                    var memberName = member.Name;
                    var memberSetter = DynamicMemberAccessor
                                                .CreateSetter<T,object>
                                                    (memberName);
                    var delegateInvoker = DynamicCallMethodExpressionTreeInvokerHelper
                                                .CreateDelegate
                                                        (
                                                            methodInfo
                                                        );
                    var settingValue = delegateInvoker
                                            .DynamicInvoke(settingValueText);
                    memberSetter(r, settingValue);
                }
            }
            return r;
        }

        public static T GetAppSettingValueByPropertyName<T>
                            (
                                Func<string, T> parseProcessFunc
                                , Func<T> defaultValueFactoryProcessFunc
                                , [CallerMemberName]
                                  string settingKeyPropertyName = null
                            )
        {
            return
                GetAppSettingValueByKey<T>
                    (
                        parseProcessFunc
                        , defaultValueFactoryProcessFunc
                        , settingKeyPropertyName
                    );
        }
        public static T GetAppSettingValueByKey<T>
                    (
                        Func<string, T> parseProcessFunc
                        , Func<T> defaultValueFactoryProcessFunc
                        , string settingKey
                    )
        {
            var r = default(T);
            var settingValue = ConfigurationManager.AppSettings[settingKey];
            if (!settingValue.IsNullOrEmptyOrWhiteSpace())
            {
                r = parseProcessFunc(settingValue);
            }
            else
            {
                r = defaultValueFactoryProcessFunc();
            }
            return r;
        }
    }
}
