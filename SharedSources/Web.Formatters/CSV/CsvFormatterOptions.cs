﻿#if NETCOREAPP

namespace Microshaoft.Web
{
    using System.Text;
    public class CsvFormatterOptions
    {
        public bool UseSingleLineHeaderInCsv { get; set; } = true;

        public string CsvColumnsDelimiter { get; set; } = ",";

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz";

        public string DateFormat { get; set; } = "yyyy-MM-ddzzz";

        public string TimeFormat { get; set; } = "HH:mm:ss.FFFFFFFzzz";

        //避免 Excel CSV 科学计数法
        public string DigitsTextSuffix { get; set; } = "";//"\t";

        public int MinExclusiveLengthDigitsTextSuffix { get; set; } = 10;//"\t";

        public bool IncludeExcelDelimiterHeader { get; set; } = false;
    }
}
#endif