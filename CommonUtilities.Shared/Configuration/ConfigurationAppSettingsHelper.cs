#if !XAMARIN && NETFRAMEWORK4_X
namespace Microshaoft
{
    using System;
    using System.Configuration;
    using System.Runtime.CompilerServices;
    using System.Reflection;
    using System.Linq;
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class ConfigurationAppSettingAttribute : Attribute
    {
        public string SettingKey;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class AllowRunTimeOverrideValueAttribute : Attribute
    {

    }

    public static class ConfigurationAppSettingsHelper
    {
        public static bool SetAppSettingValue<T>
                                (
                                    T target
                                    , string settingKey
                                    , string settingValueText
                                    , bool needPredictAllowRunTimeOverrideValue = true
                                )
                                    where T : new()
        {
            var r = false;

            var member = typeof(T)
                            .GetCustomAttributedMembers
                                    <ConfigurationAppSettingAttribute>
                                        (
                                            (x, y, z) =>
                                            {
                                                var rr = false;
                                                settingKey = z.SettingKey;
                                                if (settingKey.IsNullOrEmptyOrWhiteSpace())
                                                {
                                                    settingKey = y.Name;
                                                }
                                                if (rr)
                                                {
                                                    if (needPredictAllowRunTimeOverrideValue)
                                                    {
                                                        var attributeType = typeof(AllowRunTimeOverrideValueAttribute);
                                                        var attribute = y.GetCustomAttribute(attributeType);
                                                        if (attribute == null)
                                                        {
                                                            rr = false;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    rr = false;
                                                }
                                                return rr;
                                            }
                                        ).FirstOrDefault();
            if (member != null)
            {
                SetMemberValue<T>(target, member, settingValueText);
            }
            return r;
        }


        public static T GetAppSettingsByMapFromConfig<T>() where T : new()
        {
            T r = new T();
            Type type = typeof(T);
            string settingKey = string.Empty;
            var members = type
                            .GetCustomAttributedMembers
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
                var needSet = true;
                if (settingValueText.IsNullOrEmptyOrWhiteSpace())
                {
                    Type memberType = null;
                    if (member is PropertyInfo)
                    {
                        var propertyInfo = member as PropertyInfo;
                        memberType = propertyInfo.PropertyType;
                        var parameters = propertyInfo.GetIndexParameters();
                        if (parameters != null && parameters.Length > 0)
                        {
                            memberType = null;
                        }
                    }
                    else if (member is FieldInfo)
                    {
                        var fieldInfo = member as FieldInfo;
                        memberType = fieldInfo.FieldType;
                    }
                    if (memberType.IsValueType)
                    {
                        needSet = false;
                    }

                }
                if (needSet)
                {
                    SetMemberValue(r, member, settingValueText);
                }

            }
            return r;
        }

        private static bool SetMemberValue<T>
                                (
                                    T target
                                    , MemberInfo member
                                    , string settingValueText
                                )
                                    where T : new()
        {
            var r = false;
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
            var memberName = member.Name;
            var memberSetter = DynamicExpressionTreeHelper
                                        .CreateMemberSetter//<T, object>
                                            (typeof(T), memberName);
            var methodInfo = memberValueType
                                    .GetMethod
                                        (
                                            "Parse"
                                            , new Type[] { typeof(string) }
                                        );

            if (methodInfo != null)
            {
                var delegateInvoker = DynamicExpressionTreeHelper
                                            .CreateDelegate
                                                    (
                                                        methodInfo
                                                    );
                var settingValue = delegateInvoker
                                        .DynamicInvoke(settingValueText);
                memberSetter(target, settingValue);
            }
            else
            {
                memberSetter(target, (object)settingValueText);
            }

            r = true;
            return r;
        }

        public static T GetAppSettingValueByKeyMapToMemberName<T>
                            (
                                Func<string, T> parseProcessFunc
                                , Func<T> defaultValueFactoryProcessFunc
                                , [CallerMemberName]
                                  string settingKeyMemberName = null
                            )
        {
            return
                GetAppSettingValueByKey<T>
                    (
                        parseProcessFunc
                        , defaultValueFactoryProcessFunc
                        , settingKeyMemberName
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
#endif

