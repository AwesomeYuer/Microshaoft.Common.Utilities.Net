#if NETCOREAPP2_X

namespace Microshaoft.Web
{
    using System.Text;
    public class CsvFormatterOptions
    {
        public bool UseSingleLineHeaderInCsv { get; set; } = true;

        public string CsvDelimiter { get; set; } = ";";

        public Encoding Encoding { get; set; } = Encoding.Default;

        public bool IncludeExcelDelimiterHeader { get; set; } = false;
    }
}
#endif