
namespace Test3
{
    using Microshaoft;
    using System;
    using System.Xml;
    using System.Xml.Linq;
    class Program
    {
        public static void Main()
        {
            var errors = 0;
            var xsd =
@"<xsd:schema xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
    <xsd:element name='Root'>
        <xsd:complexType>
            <xsd:sequence>
                <xsd:element name='Child1' minOccurs='1' maxOccurs='1'/>
                <xsd:element name='Child2' minOccurs='1' maxOccurs='1'>
                    <xsd:complexType>
                        <xsd:simpleContent>
                            <xsd:extension base='xsd:string'>
                                <xsd:attribute name='Att1' default='Att1 Default Value'/>
                            </xsd:extension>
                        </xsd:simpleContent>
                    </xsd:complexType>
                </xsd:element>
            </xsd:sequence>
        </xsd:complexType>
    </xsd:element>
</xsd:schema>"
;
            XDocument xd = new XDocument
                        (
                            new XElement
                                    (
                                        "Root",
                                        new XElement("Child1", "c1"),
                                        new XElement("Child3", "c2"),
                                        new XElement("Child1", "c1"),
                                        new XElement("Child3", "c2"),
                                        new XElement("Child3", "data3"),
                                        new XElement("Child2", "data4"),
                                        new XElement("Info5", "info5"),
                                        new XElement("Info6", "info6"),
                                        new XElement("Info7", "info7"),
                                        new XElement("Info8", "info8")
                                    )
                        );
            var r = XmlValidationHelper.XsdValidateXml
                    (
                        xd
                        , ""
                        , xsd
                        , out errors
                    //, (x, y) =>
                    //{
                    //    Console.WriteLine("{0}", y.Exception);
                    //}
                    );
            Console.WriteLine("============== XsdValidateXml By XDocument {0}, {1} errors", r, errors);
            r = XmlValidationHelper.XsdValidateXml
                    (
                        xd
                        , ""
                        , xsd
                        , out errors
                        , (x, y) =>
                        {
                            Console.WriteLine("{0}", y.Exception);
                        }
                    );
            Console.WriteLine("============== XsdValidateXml By XDocument {0}, {1} errors", r, errors);
            Console.WriteLine("==========================================================================");
            var xml = xd.ToString();
            r = XmlValidationHelper.XsdValidateXml
                (
                    xml
                    , null //"http://www.contoso.com/books"
                    , xsd
                    , out errors
                    , false
                    , (x, y) =>
                    {
                        Console.WriteLine("***Validation error");
                        Console.WriteLine("\tSeverity:{0}", y.Severity);
                        Console.WriteLine("\tMessage  :{0}", y.Message);
                    }
                    , (x) =>
                    {
                        Console.WriteLine("{0}", x);
                        return false;
                    }
                );
            Console.WriteLine("============== XsdValidateXml By Xml(XmlReader) {0}, {1} errors", r, errors);
            Console.WriteLine("==========================================================================");
            Console.WriteLine("press any key to continue ...");
            Console.ReadLine();
            xml =
@"<bookstore>
    <book genre=""autobiography"" publicationdate=""1981"" ISBN=""1-861003-11-0"">
        <title>The Autobiography of Benjamin Franklin</title>
            <author>
                <first-name>Benjamin</first-name>
                <last-name>Franklin</last-name>
            </author>
            <price>8.99</price>
    </book>
    <book publicationdate=""1967"" ISBN=""0-201-63361-2"">
        <title>The Confidence Man</title>
        <author>
            <first-name>Herman</first-name>
            <last-name>Melville</last-name>
        </author>
        <price>11.99</price>
    </book>
    <book publicationdate=""1991"" ISBN=""1-861001-57-6"">
        <title>The Gorgias</title>
        <author>
            <name>Plato</name>
        </author>
        <price>9.99</price>
    </book>
</bookstore>
";
            xsd =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
<!-- <xs:schema attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" targetNamespace=""http://www.contoso.com/books"" xmlns:xs=""http://www.w3.org/2001/XMLSchema""> -->
    <xs:element name=""bookstore"">
        <xs:complexType>
            <xs:sequence>
                <xs:element maxOccurs=""unbounded"" name=""book"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""title"" type=""xs:string"" />
                            <xs:element name=""author"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element minOccurs=""0"" name=""name"" type=""xs:string"" />
                                        <xs:element minOccurs=""0"" name=""first-name"" type=""xs:string"" />
                                        <xs:element minOccurs=""0"" name=""last-name"" type=""xs:string"" />
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                            <xs:element name=""price"" type=""xs:decimal"" />
                        </xs:sequence>
                        <xs:attribute name=""genre"" type=""xs:string"" use=""required"" />
                        <xs:attribute name=""publicationdate"" type=""xs:unsignedShort"" use=""required"" />
                        <xs:attribute name=""ISBN"" type=""xs:string"" use=""required"" />
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>
";
            r = XmlValidationHelper.XsdValidateXml
                (
                    xml
                    , null //"http://www.contoso.com/books"
                    , xsd
                    , out errors
                    //, (x, y) =>
                    //{
                    //    Console.WriteLine("***Validation error");
                    //    Console.WriteLine("\tSeverity:{0}", y.Severity);
                    //    Console.WriteLine("\tMessage  :{0}", y.Message);
                    //}
                    //, (x) =>
                    //{
                    //    Console.WriteLine("{0}", x);
                    //    return false;
                    //}
                    //, true
                    );
            Console.WriteLine("============== XsdValidateXml By Xml(XmlReader) {0}, {1} errors", r, errors);
            r = XmlValidationHelper.XsdValidateXml
                (
                    xml
                    , null //"http://www.contoso.com/books"
                    , xsd
                    , out errors
                    , true
                    , (x, y) =>
                    {
                        Console.WriteLine("***Validation error");
                        Console.WriteLine("\tSeverity:{0}", y.Severity);
                        Console.WriteLine("\tMessage  :{0}", y.Message);
                    }
                    , (x) =>
                    {
                        Console.WriteLine("{0}", x);
                        return false;
                    }
                    , (x) =>
                    {
                        Console.WriteLine("{0}", x);
                        return false;
                    }
                    , (x) =>
                    {
                        Console.WriteLine("{0}", x);
                        return false;
                    }
                );
            Console.WriteLine("============== XsdValidateXml By Xml(XmlReader) {0}, {1} errors", r, errors);
            Console.WriteLine("==========================================================================");
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            r = XmlValidationHelper.XsdValidateXml
                    (
                        xmlDocument
                        , "" //"http://www.contoso.com/books"
                        , xsd
                        , out errors
                    //, (x, y) =>
                    //{
                    //    Console.WriteLine("***Validation error");
                    //    Console.WriteLine("\tSeverity:{0}", y.Severity);
                    //    Console.WriteLine("\tException  :{0}", y.Exception);
                    //}
                    );
            Console.WriteLine("============== XsdValidateXml By XmlDocument {0}, {1} errors", r, errors);
            r = XmlValidationHelper.XsdValidateXml
                    (
                        xmlDocument
                        , "" //"http://www.contoso.com/books"
                        , xsd
                        , out errors
                        , (x, y) =>
                        {
                            Console.WriteLine("***Validation error");
                            Console.WriteLine("\tSeverity:{0}", y.Severity);
                            Console.WriteLine("\tException  :{0}", y.Exception);
                        }
                    );
            Console.WriteLine("============== XsdValidateXml By XmlDocument {0}, {1} errors", r, errors);
            Console.WriteLine("==========================================================================");
            Console.WriteLine("Validation finished");
            Console.ReadLine();
        }
    }
}
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    public static class XmlValidationHelper
    {
        public static bool XsdValidateXml
                (
                    XDocument xDocument
                    , XmlSchemaSet xmlSchemaSet
                    , out int errors
                    , ValidationEventHandler validationEventHandlerAction = null
                )
        {
            var exceptions = 0;
            var r = true;
            xDocument.Validate
                        (
                            xmlSchemaSet
                            , (x, y) =>
                            {
                                r = false;
                                exceptions++;
                                if (validationEventHandlerAction != null)
                                {
                                    validationEventHandlerAction(x, y);
                                }
                            }
                            , true
                        );
            errors = exceptions;
            return r;
        }
        public static bool XsdValidateXml
                        (
                            XDocument xDocument
                            , string targetNamespace
                            , string xsd
                            , out int errors
                            , ValidationEventHandler validationEventHandlerAction = null
                        )
        {
            XmlSchemaSet xmlSchemaSet = GetXmlSchemaSet(targetNamespace, xsd);
            var r = XsdValidateXml
                    (
                        xDocument
                        , xmlSchemaSet
                        , out errors
                        , validationEventHandlerAction
                    );
            return r;
        }
        public static bool XsdValidateXml
                (
                    XmlDocument xmlDocument
                    , XmlSchemaSet xmlSchemaSet
                    , out int errors
                    , ValidationEventHandler validationEventHandlerAction = null
                )
        {
            xmlDocument.Schemas = xmlSchemaSet;
            var exceptions = 0;
            var r = true;
            xmlDocument.Validate
                (
                    (x, y) =>
                    {
                        r = false;
                        exceptions++;
                        if (validationEventHandlerAction != null)
                        {
                            validationEventHandlerAction(x, y);
                        }
                    }
                );
            errors = exceptions;
            return r;
        }
        public static bool XsdValidateXml
                        (
                            XmlDocument xmlDocument
                            , string targetNamespace
                            , string xsd
                            , out int errors
                            , ValidationEventHandler validationEventHandlerAction = null
                        )
        {
            var xmlSchemaSet = GetXmlSchemaSet(targetNamespace, xsd);
            var r = XsdValidateXml
                        (
                            xmlDocument
                            , xmlSchemaSet
                            , out errors
                            , validationEventHandlerAction
                        );
            return r;
        }
        public static bool XsdValidateXml
                    (
                        string xml
                        , out int errors
                        , XmlReaderSettings xmlReaderValidationSettings
                        , bool caughtExceptionOnlyOnce = false
                        , ValidationEventHandler validationEventHandlerAction = null
                        , Func<XmlSchemaValidationException, bool> onCaughtXmlSchemaValidationExceptionProcessFunc = null
                        , Func<XmlSchemaException, bool> onCaughtXmlSchemaExceptionProcessFunc = null
                        , Func<Exception, bool> onCaughtExceptionProcessFunc = null
                    )
        {
            var r = true;
            bool reThrow = false;
            var exceptions = 0;
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = XmlReader.Create(stringReader, xmlReaderValidationSettings))
                {
                    if (validationEventHandlerAction != null)
                    {
                        xmlReaderValidationSettings.ValidationEventHandler += validationEventHandlerAction;
                    }
                    bool readed = false;
                    var func = new Func<bool>
                                (
                                    () =>
                                    {
                                        try
                                        {
                                            readed = xmlReader.Read();
                                        }
                                        catch (XmlSchemaValidationException xsve)
                                        {
                                            r = false;
                                            exceptions++;
                                            if (onCaughtXmlSchemaValidationExceptionProcessFunc != null)
                                            {
                                                reThrow = onCaughtXmlSchemaValidationExceptionProcessFunc(xsve);
                                            }
                                            if (reThrow)
                                            {
                                                //xsve = new XmlSchemaValidationException("ReThrowInnerException", xsve);
                                                //throw xsve;
                                                throw;
                                            }
                                            if (caughtExceptionOnlyOnce)
                                            {
                                                readed = false;
                                            }
                                        }
                                        catch (XmlSchemaException xsve)
                                        {
                                            r = false;
                                            exceptions++;
                                            if (onCaughtXmlSchemaExceptionProcessFunc != null)
                                            {
                                                reThrow = onCaughtXmlSchemaExceptionProcessFunc(xsve);
                                            }
                                            if (reThrow)
                                            {
                                                //xsve = new XmlSchemaException("ReThrowInnerException", xsve);
                                                //throw xsve;
                                                throw;
                                            }
                                            if (caughtExceptionOnlyOnce)
                                            {
                                                readed = false;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            r = false;
                                            exceptions++;
                                            if (onCaughtExceptionProcessFunc != null)
                                            {
                                                reThrow = onCaughtExceptionProcessFunc(e);
                                            }
                                            if (reThrow)
                                            {
                                                //xsve = new XmlSchemaValidationException("ReThrowInnerException", xsve);
                                                //throw xsve;
                                                throw;
                                            }
                                            if (caughtExceptionOnlyOnce)
                                            {
                                                readed = false;
                                            }
                                        }
                                        return readed;
                                    }
                                );
                    while
                    (
                        func()
                    ) ;
                    errors = exceptions;
                }
            }
            return r;
        }
        public static bool XsdValidateXml
                            (
                                string xml
                                , string targetNamespace
                                , string xsd
                                , out int errors
                                , bool caughtExceptionOnlyOnce = false
                                , ValidationEventHandler validationEventHandlerAction = null
                                , Func<XmlSchemaValidationException, bool> onCaughtXmlSchemaValidationExceptionProcessFunc = null
                                , Func<XmlSchemaException, bool> onCaughtXmlSchemaExceptionProcessFunc = null
                                , Func<Exception, bool> onCaughtExceptionProcessFunc = null
                            )
        {
            XmlReaderSettings xmlReaderSettings = GetXmlReaderValidationSettings(targetNamespace, xsd);
            var r = XsdValidateXml
                        (
                            xml
                            , out errors
                            , xmlReaderSettings
                            , caughtExceptionOnlyOnce
                            , validationEventHandlerAction
                            , onCaughtXmlSchemaValidationExceptionProcessFunc
                            , onCaughtXmlSchemaExceptionProcessFunc
                            , onCaughtExceptionProcessFunc
                        );
            return r;
        }
        public static XmlReaderSettings GetXmlReaderValidationSettings
                                            (
                                                string targetNamespace
                                                , string xsd
                                                , ValidationType validationType = ValidationType.Schema
                                                , XmlSchemaValidationFlags xmlSchemaValidationFlags =
                                                                                    XmlSchemaValidationFlags.AllowXmlAttributes
                                                                                    | XmlSchemaValidationFlags.AllowXmlAttributes
                                                                                    | XmlSchemaValidationFlags.ProcessIdentityConstraints
                                                                                    | XmlSchemaValidationFlags.ProcessInlineSchema
                                                                                    | XmlSchemaValidationFlags.ProcessSchemaLocation
                                                                                    | XmlSchemaValidationFlags.ReportValidationWarnings
                                                , ValidationEventHandler validationEventHandlerAction = null
                                            )
        {
            XmlSchemaSet xmlSchemaSet = GetXmlSchemaSet(targetNamespace, xsd);
            XmlReaderSettings xmlReaderValidationSettings = new XmlReaderSettings();
            xmlReaderValidationSettings.ValidationType = validationType;
            xmlReaderValidationSettings.ValidationFlags = xmlSchemaValidationFlags;
            xmlReaderValidationSettings.Schemas.Add(xmlSchemaSet);
            if (validationEventHandlerAction != null)
            {
                xmlReaderValidationSettings.ValidationEventHandler += validationEventHandlerAction;
            }
            return xmlReaderValidationSettings;
        }
        public static XmlSchemaSet GetXmlSchemaSet(string targetNamespace, string xsd)
        {
            using (var stringReader = new StringReader(xsd))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
                    xmlSchemaSet.Add(targetNamespace, xmlReader);
                    return xmlSchemaSet;
                }
            }
        }
    }
}
