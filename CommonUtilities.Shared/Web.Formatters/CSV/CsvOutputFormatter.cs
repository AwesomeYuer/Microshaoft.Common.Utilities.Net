#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Original code taken from
    /// http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format
    /// Adapted for ASP.NET Core and uses ; instead of , for delimiters
    /// </summary>
    public class CsvOutputFormatter : OutputFormatter
    {

        private readonly Regex _digitsRegex = new Regex(@"^\d+$");
        private readonly byte[] _utf8HeaderBytes = new byte[]
                                                        {
                                                            0xEF
                                                            , 0xBB
                                                            , 0xBF
                                                        };
        //private readonly CsvFormatterOptions _defaultCsvFormatterOptions = new CsvFormatterOptions();
        public string ContentType
        {
            get;
            private set;
        }
        public CsvOutputFormatter
                        (
                            //CsvFormatterOptions
                            //        csvFormatterOptions
                        )
        {
            ContentType = "text/csv";
            SupportedMediaTypes
                    .Add
                        (
                            Microsoft
                                .Net
                                .Http
                                .Headers
                                .MediaTypeHeaderValue
                                .Parse("text/csv")
                        );
            //csvFormatterOptions = csvFormatterOptions
            //                ??
            //                    throw
            //                        new
            //                            ArgumentNullException
            //                                (
            //                                    nameof(csvFormatterOptions)
            //                                );
        }

        protected override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw
                    new ArgumentNullException("type");
            }
            return
                IsTypeOfIEnumerable(type);
        }
        private bool IsTypeOfIEnumerable(Type type)
        {
            if (type == null)
            {
                throw
                    new ArgumentNullException("type");
            }
            return
                typeof(IEnumerable)
                        .IsAssignableFrom(type);
        }

        private IConfiguration _configuration;
        
        private readonly object _locker = new object();

        public async override Task WriteResponseBodyAsync
                                    (
                                        OutputFormatterWriteContext
                                                                context
                                    )
        {
            var csvFormatterOptions = new CsvFormatterOptions();
            string getValue(JToken jToken, string format = null, string digitsTextSuffix = null)
            {
                var @value = string.Empty;
                if (jToken != null)
                {
                    if (jToken.Type == JTokenType.Date)
                    {
                        var dateTime = (DateTime) jToken;
                        //@value = ((DateTime) jValue).ToString("yyyy-MM-ddTHH:mm:ss.fffff");
                        if (format.IsNullOrEmptyOrWhiteSpace())
                        {
                            format = csvFormatterOptions.DateTimeFormat;
                        }
                        if (!format.IsNullOrEmptyOrWhiteSpace())
                        {
                            @value = $@"""{dateTime.ToString(format)}""";
                        }
                        else
                        {
                            @value = $@"""{dateTime.ToString()}""";
                        }
                    }
                    else
                    {
                        @value = jToken.ToString();
                        if (jToken.Type == JTokenType.String)
                        {
                            if
                                (
                                    digitsTextSuffix == null
                                )
                            {
                                digitsTextSuffix = csvFormatterOptions
                                                            .DigitsTextSuffix;
                            }
                            if
                                (
                                    !string
                                        .IsNullOrEmpty
                                            (
                                                digitsTextSuffix
                                            )
                                    &&
                                    _digitsRegex.IsMatch(@value)
                                )
                            {
                                if
                                    (
                                        (
                                            @value
                                                .Length
                                            >
                                            csvFormatterOptions
                                                .MinExclusiveLengthDigitsTextSuffix
                                        )
                                        ||
                                        @value
                                            .StartsWith("0")
                                    )
                                {
                                    @value += csvFormatterOptions
                                                        .DigitsTextSuffix;
                                }
                            }
                            else
                            {
                                @value = @value.Replace(@"""", @"""""");
                                //Check if the value contains a delimiter and place it in quotes if so
                                if
                                    (
                                        @value
                                            .Contains
                                                    (
                                                        csvFormatterOptions
                                                                .CsvColumnsDelimiter
                                                    )
                                        ||
                                        @value.Contains("\r")
                                        ||
                                        @value.Contains("\n")
                                    )
                                {
                                    @value = $@"""{@value}""";
                                }
                            }
                        }
                    }
                }
                return
                    @value;
            }
            var httpContext = context
                                .HttpContext;
            var request = httpContext
                                .Request;
            var httpMethod = $"http{request.Method}";
            var routeName = (string) httpContext
                                            .GetRouteData()
                                            .Values["routeName"];
            _locker
                .LockIf
                    (
                        () =>
                        {
                            return
                                (_configuration == null);
                        }
                        , () =>
                        {
                            _configuration = (IConfiguration)
                                                    httpContext
                                                            .RequestServices
                                                            .GetService
                                                                (
                                                                    typeof(IConfiguration)
                                                                );

                        }
                    );
            if 
                (
                    _configuration
                                .TryGetSection
                                    (
                                        "ExportCsvFormatter"
                                        , out var exportCsvFormatterConfiguration
                                    )
                )
            {
                IConfigurationSection section;
                if
                    (
                        exportCsvFormatterConfiguration
                                .TryGetSection
                                    (
                                        nameof(csvFormatterOptions.CsvColumnsDelimiter)
                                        , out section
                                    )
                    )
                {
                    csvFormatterOptions
                            .CsvColumnsDelimiter = section.Value;
                }
                if
                    (
                        exportCsvFormatterConfiguration
                                .TryGetSection
                                    (
                                        nameof(csvFormatterOptions.DateFormat)
                                        , out section
                                    )
                    )
                {
                    csvFormatterOptions
                            .DateFormat = section.Value;
                }
                if
                    (
                        exportCsvFormatterConfiguration
                                .TryGetSection
                                    (
                                        nameof(csvFormatterOptions.DateTimeFormat)
                                        , out section
                                    )
                    )
                {
                    csvFormatterOptions
                            .DateTimeFormat = section.Value;
                }
                if
                    (
                        exportCsvFormatterConfiguration
                                .TryGetSection
                                    (
                                        nameof(csvFormatterOptions.DigitsTextSuffix)
                                        , out section
                                    )
                    )
                { 
                    csvFormatterOptions
                            .DigitsTextSuffix = section.Value;
                }
                if
                    (
                        exportCsvFormatterConfiguration
                                .TryGetSection
                                    (
                                        nameof(csvFormatterOptions.Encoding)
                                        , out section
                                    )
                    )
                {
                    csvFormatterOptions
                            .Encoding = Encoding.GetEncoding(section.Value);
                }
                if
                    (
                        exportCsvFormatterConfiguration
                                .TryGetSection
                                    (
                                        nameof(csvFormatterOptions.IncludeExcelDelimiterHeader)
                                        , out section
                                    )
                    )
                {
                    csvFormatterOptions
                            .IncludeExcelDelimiterHeader = section.Get<bool>();
                }
                if
                    (
                        exportCsvFormatterConfiguration
                                .TryGetSection
                                    (
                                        nameof(csvFormatterOptions.UseSingleLineHeaderInCsv)
                                        , out section
                                    )
                    )
                {
                   csvFormatterOptions
                            .UseSingleLineHeaderInCsv = section.Get<bool>();
                }
            }
            var encodingName = (string) request.Query["e"];
            Encoding e = null;
            if (!encodingName.IsNullOrEmptyOrWhiteSpace())
            {
                e = Encoding
                        .GetEncoding(encodingName);
            }
            else
            {
                e = csvFormatterOptions.Encoding;
            }
            var response = httpContext
                                    .Response;
            var downloadFileName = $"{routeName}.csv";
            var downloadFileNameConfiguration =
                    _configuration
                            .GetSection
                                (
                                    $"Routes:{routeName}:{httpMethod}:Exporting:DownloadFileName"
                                );
            if (downloadFileNameConfiguration.Exists())
            {
                downloadFileName = downloadFileNameConfiguration.Value;
            }
            downloadFileName = HttpUtility
                                        .UrlEncode(downloadFileName, e);
            response
                    .Headers
                    .Add
                        (
                            "Content-Disposition"
                            , $@"attachment; filename=""{downloadFileName}"""
                        );
            using
                (
                    var streamWriter = new StreamWriter
                                                (
                                                    response.Body
                                                    , e
                                                )
                )
            {
                if (e.GetType() == Encoding.UTF8.GetType())
                {
                    await
                        response
                            .Body
                            .WriteAsync
                                (
                                    _utf8HeaderBytes
                                );
                }
                if (csvFormatterOptions.IncludeExcelDelimiterHeader)
                {
                    //乱码
                    await
                        streamWriter
                            .WriteLineAsync
                                (
                                    $"sep ={csvFormatterOptions.CsvColumnsDelimiter}"
                                );
                }
                if (context.Object is JArray jArray)
                {
                    var allOutputColumnsConfiguration =
                            _configuration
                                    .GetSection
                                        (
                                            $"Routes:{routeName}:{httpMethod}:Exporting:OutputColumns"
                                        );
                    (
                        string ColumnName
                        , string ColumnTitle
                        , string DataFormat
                        , string DigitsTextSuffix
                    )
                        [][] allOutputColumns = null;
                    if (allOutputColumnsConfiguration.Exists())
                    {
                        allOutputColumns =
                            allOutputColumnsConfiguration
                                    .GetChildren()
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                return
                                                    x
                                                        .GetChildren()
                                                        .Select
                                                            (
                                                                (xx) =>
                                                                {
                                                                    var columnName = xx
                                                                                        .GetValue<string>
                                                                                                ("ColumnName");
                                                                    var columnTitle = xx
                                                                                        .GetValue
                                                                                                (
                                                                                                    "ColumnTitle"
                                                                                                    , columnName
                                                                                                );
                                                                    var dataFormat = xx
                                                                                        .GetValue
                                                                                                (
                                                                                                    "DataFormat"
                                                                                                    , string.Empty
                                                                                                );
                                                                    var digitsTextSuffix = xx
                                                                                        .GetValue<string>
                                                                                                (
                                                                                                    "DigitsTextSuffix"
                                                                                                    , null
                                                                                                );
                                                                    return
                                                                        (
                                                                            ColumnName: columnName
                                                                            , ColumnTitle: columnTitle
                                                                            , DataFormat: dataFormat
                                                                            , DigitsTextSuffix: digitsTextSuffix
                                                                        );
                                                                }
                                                            )
                                                        .ToArray();
                                            }
                                        )
                                    .ToArray();
                        (
                            string ColumnName
                            , string ColumnTitle
                            , string DataFormat
                            , string DigitsTextSuffix
                        )
                            [] outputColumns = null;
                        var i = 0;
                        foreach (JObject jObject in jArray)
                        {
                            if (i == 0)
                            {
                                if (csvFormatterOptions.UseSingleLineHeaderInCsv)
                                {
                                    if
                                        (
                                            allOutputColumns == null
                                            ||
                                            i >= allOutputColumns.Length
                                        )
                                    {
                                        var j = 0;
                                        var columnsHeaderLine =
                                                        jObject
                                                            .Properties()
                                                            .Aggregate
                                                                (
                                                                    string.Empty
                                                                    , (x, y) =>
                                                                    {
                                                                        if (j > 0)
                                                                        {
                                                                            x += csvFormatterOptions
                                                                                    .CsvColumnsDelimiter;
                                                                        }
                                                                        x += y.Name;
                                                                        j++;
                                                                        return
                                                                                x;
                                                                    }
                                                                );
                                        await
                                            streamWriter
                                                .WriteLineAsync
                                                        (
                                                            columnsHeaderLine
                                                        );
                                    }
                                    else
                                    {
                                        if (i < allOutputColumns.Length)
                                        {
                                            outputColumns = allOutputColumns[i];
                                            var j = 0;
                                            var columnsHeaderLine =
                                                        outputColumns
                                                                .Aggregate
                                                                    (
                                                                        string.Empty
                                                                        , (x, y) =>
                                                                        {
                                                                            if (j > 0)
                                                                            {
                                                                                x += csvFormatterOptions
                                                                                        .CsvColumnsDelimiter;
                                                                            }
                                                                            x += y.ColumnTitle;
                                                                            j ++;
                                                                            return
                                                                                    x;
                                                                        }
                                                                    );
                                            await
                                                streamWriter
                                                    .WriteLineAsync
                                                            (
                                                                columnsHeaderLine
                                                            );
                                        }
                                    }
                                }
                            }
                            string line = string.Empty;
                            if (outputColumns == null)
                            {
                                var jProperties = jObject.Properties();
                                var j = 0;
                                foreach (var jProperty in jProperties)
                                {
                                    if (j > 0)
                                    {
                                        line += csvFormatterOptions
                                                        .CsvColumnsDelimiter;
                                    }
                                    var jToken = jProperty.Value;
                                    line += getValue(jToken);
                                    jToken = null;
                                    j ++;
                                }
                                jProperties = null;
                            }
                            else
                            {
                                var j = 0;
                                foreach (var (columnName, columnTitle, dataFormat, digitsTextSuffix) in outputColumns)
                                {
                                    if (j > 0)
                                    {
                                        line += csvFormatterOptions
                                                        .CsvColumnsDelimiter;
                                    }
                                    if
                                        (
                                            jObject
                                                .TryGetValue
                                                    (
                                                        columnName
                                                        , StringComparison
                                                                .OrdinalIgnoreCase
                                                        , out JToken jToken
                                                    )
                                        )
                                    {
                                        line += getValue(jToken, dataFormat, digitsTextSuffix);
                                    }
                                    j++;
                                }
                            }
                            await
                                streamWriter
                                    .WriteLineAsync(line);
                            //await
                            //    streamWriter
                            //        .FlushAsync();
                            i ++;
                        }
                        jArray = null;
                    }
                }
                await
                    streamWriter
                            .FlushAsync();
                streamWriter.Close();
            }
        }
    }
}
#endif
