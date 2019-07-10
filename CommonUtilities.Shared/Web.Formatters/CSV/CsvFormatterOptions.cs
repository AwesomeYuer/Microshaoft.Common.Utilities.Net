#if NETCOREAPP2_X

namespace Microshaoft.Web
{
    using System.Text;
    public class CsvFormatterOptions
    {
        public bool UseSingleLineHeaderInCsv { get; set; } = true;

        public string CsvColumnsDelimiter { get; set; } = ",";

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz";

        //避免 Excel CSV 科学计数法
        public string DigitsTextSuffix { get; set; } = "";//"\t";

        public bool IncludeExcelDelimiterHeader { get; set; } = false;
    }
}
#endif