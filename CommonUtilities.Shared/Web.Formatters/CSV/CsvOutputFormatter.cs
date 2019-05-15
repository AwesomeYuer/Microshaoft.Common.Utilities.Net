#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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

        public CsvOutputFormatter(CsvFormatterOptions csvFormatterOptions)
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
            _options = csvFormatterOptions ?? throw new ArgumentNullException(nameof(csvFormatterOptions));
        }

        protected override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return IsTypeOfIEnumerable(type);
        }

        private bool IsTypeOfIEnumerable(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return typeof(IEnumerable).IsAssignableFrom(type);
        }
        public async override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var httpContext = context
                                .HttpContext;
            var request = httpContext
                                .Request;
            var encodingName = (string) request.Query["e"];

            Encoding e = null;
            if (!encodingName.IsNullOrEmptyOrWhiteSpace())
            {
                e = Encoding.GetEncoding(encodingName);
            }
            else
            {
                e = Encoding.UTF8;
            }
            var response = httpContext
                                .Response;
            //httpContext.Features
            
            using
                (
                    var streamWriter = new StreamWriter
                                                (
                                                    response.Body
                                                    , e
                                                )
                )
            {
                await
                    response
                        .Body
                        .WriteAsync
                            (
                                new byte[] { 0xEF, 0xBB, 0xBF }
                            );

                if (_options.IncludeExcelDelimiterHeader)
                {
                    await
                        streamWriter
                            .WriteLineAsync($"sep ={_options.CsvDelimiter}");
                }
                if (context.Object is JArray data)
                {
                    int i = 0;
                    foreach (JObject jObject in data)
                    {
                        var jProperties = jObject.Properties();
                        if (i == 0)
                        {
                            if (_options.UseSingleLineHeaderInCsv)
                            {
                                var propertiesNames = jProperties
                                                            .Select
                                                                (
                                                                    (x) =>
                                                                    {
                                                                        return
                                                                            x.Name;
                                                                    }
                                                                );
                                await
                                    streamWriter
                                        .WriteLineAsync
                                                (
                                                    string
                                                        .Join
                                                            (
                                                                _options
                                                                    .CsvDelimiter
                                                                , propertiesNames
                                                            )
                                                );
                            }
                        }
                        string line = string.Empty;
                        var j = 0;
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
                                if (jValue.Type == JTokenType.String)
                                {
                                    @value = jValue.ToString();
                                    @value = @value.Replace(@"""", @"""""");
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
                                    //Replace any \r or \n special characters from a new line with a space
                                    //if (@value.Contains("\r"))
                                    //{
                                    //    @value = @value.Replace("\r", " ");
                                    //}
                                    //if (@value.Contains("\n"))
                                    //{
                                    //    @value = @value.Replace("\n", " ");
                                    //}
                                }
                                else if (jValue.Type == JTokenType.Date)
                                {
                                    //@value = ((DateTime) jValue).ToString("yyyy-MM-ddTHH:mm:ss.fffff");
                                    @value = $@"""{((DateTime)jValue).ToString("yyyy-MM-ddTHH:mm:ss.fffff")}""";
                                }
                                else
                                {
                                    @value = jValue.ToString();
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