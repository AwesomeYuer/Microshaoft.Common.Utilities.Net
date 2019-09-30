#if NETCOREAPP2_X
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
        private readonly CsvFormatterOptions _options;
        public string ContentType
        {
            get;
            private set;
        }
        public CsvOutputFormatter
                        (
                            CsvFormatterOptions
                                    csvFormatterOptions
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
            _options = csvFormatterOptions
                            ??
                                throw
                                    new
                                        ArgumentNullException
                                            (
                                                nameof(csvFormatterOptions)
                                            );
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
            string getValue(JToken jToken)
            {
                var @value = string.Empty;
                if (jToken != null)
                {
                    if (jToken.Type == JTokenType.Date)
                    {
                        //@value = ((DateTime) jValue).ToString("yyyy-MM-ddTHH:mm:ss.fffff");
                        @value = $@"""{((DateTime) jToken).ToString(_options.DateTimeFormat)}""";
                    }
                    else
                    {
                        @value = jToken.ToString();
                        @value = @value.Replace(@"""", @"""""");
                        if (jToken.Type == JTokenType.String)
                        {
                            if (!string.IsNullOrEmpty(_options.DigitsTextSuffix))
                            {
                                if (_digitsRegex.IsMatch(@value))
                                {
                                    //避免在Excel中csv文本数字自动变科学计数法
                                    @value += _options.DigitsTextSuffix;
                                    //@value = $@"=""{@value}""";
                                }
                            }
                        }
                        //Check if the value contains a delimiter and place it in quotes if so
                        if
                            (
                                @value.Contains(_options.CsvColumnsDelimiter)
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
                return @value;
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
            var encodingName = (string) request.Query["e"];
            Encoding e = null;
            if (!encodingName.IsNullOrEmptyOrWhiteSpace())
            {
                e = Encoding
                        .GetEncoding(encodingName);
            }
            else
            {
                e = Encoding.UTF8;
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
            downloadFileName = HttpUtility.UrlEncode(downloadFileName, e);
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
                if (_options.IncludeExcelDelimiterHeader)
                {
                    //乱码
                    await
                        streamWriter
                            .WriteLineAsync
                                (
                                    $"sep ={_options.CsvColumnsDelimiter}"
                                );
                }
                if (context.Object is JArray jArray)
                {
                    var outputColumnsConfiguration =
                            _configuration
                                    .GetSection
                                        (
                                            $"Routes:{routeName}:{httpMethod}:Exporting:OutputColumns"
                                        );
           
                    (
                        string ColumnName
                        , string ColumnTitle
                    ) [] outputColumns = null;
                    if (outputColumnsConfiguration.Exists())
                    {
                        outputColumns =
                            outputColumnsConfiguration
                                        .GetChildren()
                                        .Select
                                            (
                                                (x) =>
                                                {
                                                    var columnName = x
                                                                        .GetValue<string>
                                                                                ("ColumnName");
                                                    var columnTitle = x
                                                                        .GetValue
                                                                                (
                                                                                    "ColumnTitle"
                                                                                    , columnName
                                                                                );
                                                    return
                                                        (
                                                            ColumnName: columnName
                                                            , ColumnTitle: columnTitle
                                                        );
                                                }                                           
                                            )
                                        .ToArray();
                        if (_options.UseSingleLineHeaderInCsv)
                        {
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
                                                                x += _options.CsvColumnsDelimiter;
                                                            }
                                                            x += y.ColumnTitle;
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
                    }
                    var i = 0;
                    foreach (JObject jObject in jArray)
                    {
                        if (i == 0)
                        {
                            if (_options.UseSingleLineHeaderInCsv)
                            {
                                if (outputColumns == null)
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
                                                                        x += _options
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
                                    line += _options.CsvColumnsDelimiter;
                                }
                                var jToken = jProperty.Value;
                                line += getValue(jToken);
                                jToken = null;
                                j++;
                            }
                            jProperties = null;
                        }
                        else
                        {
                            var j = 0;
                            foreach (var (columnName, columnTitle) in outputColumns)
                            {
                                if (j > 0)
                                {
                                    line += _options.CsvColumnsDelimiter;
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
                                    line += getValue(jToken);
                                }
                                j++;
                            }
                        }
                        await
                            streamWriter
                                .WriteLineAsync(line);
                        i++;
                    }
                    jArray = null;
                    await
                        streamWriter
                            .FlushAsync();
                }
            }
        }
    }
}
#endif