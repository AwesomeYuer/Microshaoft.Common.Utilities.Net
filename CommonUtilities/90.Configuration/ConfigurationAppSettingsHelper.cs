namespace Microshaoft
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Runtime.CompilerServices;
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
            var propertyType = typeof(T);
            ConfigurationAppSettingAttribute attribute = null; 
            var properties 
                    = propertyType
                            .GetProperties()
                            .Where
                                (
                                    (x) =>
                                    {
                                        var rr = false;
                                        attribute = x
                                                        .GetCustomAttributes
                                                            (typeof(ConfigurationAppSettingAttribute), true)
                                                        .FirstOrDefault() as ConfigurationAppSettingAttribute;
                                        if (attribute != null)
                                        {
                                            if (!attribute.SettingKey.IsNullOrEmptyOrWhiteSpace())
                                            {
                                                rr = true;
                                            }
                                        }
                                        return rr;
                                    }
                                );
            foreach (var property in properties)
            {
                propertyType = property.PropertyType;
                var methodInfo = propertyType
                                    .GetMethod
                                        (
                                            "Parse"
                                            , new Type[] { typeof(string) }
                                        );
                var propertyName = property.Name;
                var key = attribute.SettingKey;
                if (key.IsNullOrEmptyOrWhiteSpace())
                {
                    key = propertyName;
                }
                if (methodInfo != null)
                {
                    var settingValueText = ConfigurationManager.AppSettings[key];
                    var propertySetter = DynamicPropertyAccessor
                                                .CreateTargetSetPropertyValueAction<T>
                                                    (propertyName);
                    var delegateInvoker = DynamicCallMethodExpressionTreeInvokerHelper
                                                .CreateDelegate
                                                        (
                                                            methodInfo
                                                        );
                    var settingValue = delegateInvoker.DynamicInvoke(settingValueText);
                    propertySetter(r, settingValue);
                 }
            }
            return r;
        }

        public static T GetAppSettingValueByPropertyName<T>
                            (
                                Func<string, T> parseProcessFunc
                                , Func<T> defaultValueFactoryProcessFunc
                                , [CallerMemberName]
                                  string settingKeyPropertyName = ""
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
