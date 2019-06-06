#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Original code taken from
    /// http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format
    /// Adapted for ASP.NET Core and uses ; instead of , for delimiters
    /// </summary>
    public class CsvOutputFormatter : OutputFormatter
    {
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
        private object _locker = new object();

        public async override Task WriteResponseBodyAsync
                                    (
                                        OutputFormatterWriteContext
                                                                context
                                    )
        {
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
            var downloadFileName = routeName;
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
                                    new byte[]
                                    {
                                        0xEF
                                        , 0xBB
                                        , 0xBF
                                    }
                                );
                }
                if (_options.IncludeExcelDelimiterHeader)
                {
                    await
                        streamWriter
                            .WriteLineAsync
                                (
                                    $"sep ={_options.CsvDelimiter}"
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
                    Dictionary<string, string> outputColumns = null;
                    if (outputColumnsConfiguration.Exists())
                    {
                        outputColumns = outputColumnsConfiguration
                                                .GetChildren()
                                                .ToDictionary
                                                    (
                                                        (x) =>
                                                        {
                                                            return
                                                                x
                                                                    .GetValue<string>
                                                                            ("ColumnName");
                                                        }
                                                        , (x) =>
                                                        {
                                                            return
                                                                x
                                                                    .GetValue<string>
                                                                            ("ColumnTitle");
                                                        }
                                                        , StringComparer
                                                                .OrdinalIgnoreCase
                                                    );
                        if (_options.UseSingleLineHeaderInCsv)
                        {
                            var columnsHeaderLine =
                                            string
                                                .Join
                                                    (
                                                        _options
                                                            .CsvDelimiter
                                                        , outputColumns
                                                                    .Values
                                                    );
                            await
                                streamWriter
                                    .WriteLineAsync
                                            (
                                                columnsHeaderLine
                                            );
                        }
                    }
                    int i = 0;
                    foreach (JObject jObject in jArray)
                    {
                        var jProperties = jObject.Properties();
                        if (i == 0)
                        {
                            if (_options.UseSingleLineHeaderInCsv)
                            {
                                if (outputColumns == null)
                                {
                                    var propertiesNames =
                                            jProperties
                                                .Select
                                                    (
                                                        (x) =>
                                                        {
                                                            return
                                                                x.Name;
                                                        }
                                                    );
                                    var columnsHeaderLine = string
                                                                .Join
                                                                    (
                                                                        _options
                                                                            .CsvDelimiter
                                                                        , propertiesNames
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
                        var j = 0;
                        if (outputColumns != null)
                        {
                            jProperties = jProperties
                                                .Where
                                                    (
                                                        (x) =>
                                                        {
                                                            return
                                                                outputColumns
                                                                        .ContainsKey
                                                                            (
                                                                                x.Name
                                                                            );
                                                        }
                                                    );
                        }
                        foreach (var jProperty in jProperties)
                        {
                            if (j > 0)
                            {
                                line += _options.CsvDelimiter;
                            }
                            var jValue = jProperty.Value;
                            var @value = string.Empty;
                            if (jValue != null)
                            {
                                if (jValue.Type == JTokenType.Date)
                                {
                                    //@value = ((DateTime) jValue).ToString("yyyy-MM-ddTHH:mm:ss.fffff");
                                    @value = $@"""{((DateTime) jValue).ToString("yyyy-MM-dd HH:mm:ss.fff")}""";
                                }
                                else
                                {
                                    @value = jValue.ToString();
                                    @value = @value.Replace(@"""", @"""""");
                                    if (jValue.Type == JTokenType.String)
                                    {
                                        //避免在Excel中csv文本数字自动变科学计数法
                                        @value += "\t";
                                    }
                                    //Check if the value contans a delimiter and place it in quotes if so
                                    if
                                        (
                                            @value.Contains(_options.CsvDelimiter)
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
                            line += @value;
                            j++;
                        }
                        await
                            streamWriter
                                .WriteLineAsync(line);
                        i++;
                    }
                    await
                        streamWriter
                            .FlushAsync();
                }
            }
        }
    }
}
#endif