namespace Microshaoft
{
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Xml.Xsl;
	public static class XslCompiledTransformHelper
	{
		public static string Transform(string xsl, string xml)
		{
			var xslt = new XslCompiledTransform();
			using var xslStringReader = new StringReader(xsl);
			using var xslXmlReader = XmlReader.Create(xslStringReader);
			xslt
				.Load(xslXmlReader);
			using var xmlStringReader = new StringReader(xml);
			using var xmlReader = XmlReader.Create(xmlStringReader);
			var xmlWriterSettings = new XmlWriterSettings
			{
				//xmlWriterSettings.Encoding = Encoding.UTF8;
				Indent = true
				, OmitXmlDeclaration = true
			};
			using var stream = new MemoryStream();
			using var xmlWriter = XmlWriter
										.Create
											(
												stream
												, xmlWriterSettings
											);
			xslt
				.Transform
						(
							xmlReader
							, xmlWriter
						);
			var buffer = StreamDataHelper.ReadDataToBytes(stream);
			var e = EncodingHelper.IdentifyEncoding(stream, Encoding.Default);
			var offset = e.GetPreamble().Length;
			var s = e.GetString(buffer, offset, buffer.Length - offset);
			return s;
		}
	}
}
namespace Test
{
	using Microshaoft;
	using System;
	class Program11111
	{
		static void Main(string[] args)
		{

			string xsl =
@"<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
	<xsl:template match=""bookstore"">
		<HTML>
			<BODY>
				<TABLE BORDER=""2"">
					<TR>
						<TD>ISBN</TD>
						<TD>Title</TD>
						<TD>Price</TD>
					</TR>
					<xsl:apply-templates select=""book""/>
				</TABLE>
			</BODY>
		</HTML>
	</xsl:template>
	<xsl:template match=""book"">
		<TR>
			<TD>
				<xsl:value-of select=""@ISBN""/>
			</TD>
			<TD>
				<xsl:value-of select=""title""/>
			</TD>
			<TD>
				<xsl:value-of select=""price""/>
			</TD>
		</TR>
	</xsl:template>
</xsl:stylesheet>
";
			string xml =
@"<?xml version=""1.0""?>
<!-- This file represents a fragment of a book store inventory database -->
<bookstore>
	<book genre=""autobiography"" publicationdate=""1981"" ISBN=""1-861003-11-0"">
		<title>
			The Autobiography of Benjamin Franklin
		</title>
		<author>
			<first-name>
				Benjamin
			</first-name>
			<last-name>
				Franklin
			</last-name>
		</author>
		<price>
			8.99
		</price>
	</book>
	<book genre=""novel"" publicationdate=""1967"" ISBN=""0-201-63361-2"">
		<title>
			The Confidence Man
		</title>
		<author>
			<first-name>
				Herman
			</first-name>
			<last-name>
				Melville
			</last-name>
		</author>
		<price>
			11.99
		</price>
	</book>
	<book genre=""philosophy"" publicationdate=""1991"" ISBN=""1-861001-57-6"">
		<title>
			The Gorgias
		</title>
		<author>
			<name>
				Plato
			</name>
		</author>
		<price>
			于溪玥
		</price>
	</book>
</bookstore>";
			string s = XslCompiledTransformHelper.Transform(xsl, xml);
			Console.WriteLine(s);
			Console.ReadLine();
		}
	}

}