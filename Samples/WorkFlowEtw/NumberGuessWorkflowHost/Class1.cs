

namespace Microshaoft
{
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.IO;
    using System.Reflection;
    using System.Xaml;
    using System.Xml;
    public static class WorkFlowHelper
    {
        public static Activity XamlToActivity(string xaml)
        {
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            XamlXmlReader xamlXmlReader = new XamlXmlReader
                                                (
                                                    xmlReader
                                                    , new XamlXmlReaderSettings()
                                                    {
                                                        LocalAssembly = Assembly.GetExecutingAssembly()
                                                    }
                                                );
            XamlReader xamlReader = ActivityXamlServices.CreateReader
                                            (
                                                xamlXmlReader
                                            );
            Activity activity = ActivityXamlServices.Load
                                (
                                    xamlReader
                                    , new ActivityXamlServicesSettings()
                                    {
                                        //CompileExpressions = true
                                    }
                                );
            return activity;
        
        }
    }
}
