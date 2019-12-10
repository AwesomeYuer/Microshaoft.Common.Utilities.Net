#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    public abstract partial class 
                AbstractStoreProceduresExecutorControllerBase
                    :
                        ControllerBase
    {

        private readonly Regex _digitsRegex = new Regex(@"^\d+$");
        private readonly byte[] _utf8HeaderBytes = new byte[]
                                                        {
                                                            0xEF
                                                            , 0xBB
                                                            , 0xBF
                                                        };

        private readonly CsvFormatterOptions _csvFormatterOptions;
 
        private string GetFieldValue(IDataReader reader, int fieldIndex, string format = null)
        {
            string @value;
            var fieldType = reader.GetFieldType(fieldIndex);
            if 
                (
                    string
                        .Compare
                            (
                                reader
                                    .GetDataTypeName(fieldIndex)
                                , "Date"
                                , true
                            )
                    ==
                    0
                )
            {
                var date = reader.GetDateTime(fieldIndex);
                if (format.IsNullOrEmptyOrWhiteSpace())
                {
                    format = _csvFormatterOptions.DateFormat;
                }
                if (!format.IsNullOrEmptyOrWhiteSpace())
                {
                    @value = $@"""{date.ToString(format)}""";
                }
                else
                {
                    @value = $@"""{date.ToString()}""";
                }
            }
            else if
                (
                    fieldType
                    ==
                    typeof(DateTime)
                )
            {
                var dateTime = reader.GetDateTime(fieldIndex);
                if (format.IsNullOrEmptyOrWhiteSpace())
                {
                    format = _csvFormatterOptions.DateTimeFormat;
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
                @value = reader.GetValue(fieldIndex).ToString();
                @value = @value.Replace(@"""", @"""""");
                if (fieldType == typeof(string))
                {
                    if (!string.IsNullOrEmpty(_csvFormatterOptions.DigitsTextSuffix))
                    {
                        if (_digitsRegex.IsMatch(@value))
                        {
                            //避免在Excel中csv文本数字自动变科学计数法
                            @value += _csvFormatterOptions.DigitsTextSuffix;
                            //@value = $@"=""{@value}""";
                        }
                    }
                }
                //Check if the value contains a delimiter and place it in quotes if so
                if
                    (
                        @value.Contains(_csvFormatterOptions.CsvColumnsDelimiter)
                        ||
                        @value.Contains("\r")
                        ||
                        @value.Contains("\n")
                    )
                {
                    @value = $@"""{@value}""";
                }
            }
            return @value;
        }

        // ===============================================================================================================
        // Big Data Export CSV
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [
             Route
                 (
                     "bigdataexport/{routeName}/"
                 )
        ]
        [OperationsAuthorizeFilter(false)]
        [RequestJTokenParametersDefaultProcessFilter]
        public async Task
                             ProcessActionRequest
                                 (
                                     [FromRoute]
                                        string routeName
                                     , [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                 )
        {
            var request = HttpContext
                                .Request;
            var httpMethod = $"http{request.Method}";
            var encodingName = (string) request.Query["e"];
            Encoding e = null;
            if (!encodingName.IsNullOrEmptyOrWhiteSpace())
            {
                e = Encoding
                        .GetEncoding(encodingName);
            }
            else
            {
                e = _csvFormatterOptions.Encoding;
            }

            var response = HttpContext
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
                if (_csvFormatterOptions.IncludeExcelDelimiterHeader)
                {
                    //导致中文乱码
                    await
                        streamWriter
                            .WriteLineAsync
                                (
                                    $"sep ={_csvFormatterOptions.CsvColumnsDelimiter}"
                                );
                }
                var outputColumnsConfiguration =
                            _configuration
                                    .GetSection
                                        (
                                            $"Routes:{routeName}:{httpMethod}:Exporting:OutputColumns"
                                        );

                (
                    string ColumnName
                    , string ColumnTitle
                    , string DataFormat
                )
                    [] outputColumns = null;
                if (outputColumnsConfiguration.Exists())
                {
                    outputColumns = outputColumnsConfiguration
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
                                                                var dataFormat = x
                                                                                    .GetValue
                                                                                            (
                                                                                                "DataFormat"
                                                                                                , string.Empty
                                                                                            );
                                                                return
                                                                    (
                                                                        ColumnName: columnName
                                                                        , ColumnTitle: columnTitle
                                                                        , DataFormat: dataFormat
                                                                    );
                                                            }
                                                        )
                                                    .ToArray();
                    if (_csvFormatterOptions.UseSingleLineHeaderInCsv)
                    {
                        var j = 0;
                        var columnsHeaderLine = outputColumns
                                                        .Aggregate
                                                            (
                                                                string.Empty
                                                                , (x, y) =>
                                                                {
                                                                    if (j > 0)
                                                                    {
                                                                        x += _csvFormatterOptions
                                                                                    .CsvColumnsDelimiter;
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
                _service
                    .ProcessReaderReadRows
                        (
                            routeName
                            , parameters
                            , (resultSetIndex, reader, columns, rowIndex) =>
                            {
                                if (rowIndex == 0)
                                {
                                    if (_csvFormatterOptions.UseSingleLineHeaderInCsv)
                                    {
                                        if (outputColumns == null)
                                        {
                                            var j = 0;
                                            var columnsHeaderLine = columns
                                                                        .Aggregate
                                                                            (
                                                                                string.Empty
                                                                                , (x, y) =>
                                                                                {
                                                                                    if (j > 0)
                                                                                    {
                                                                                        x += _csvFormatterOptions
                                                                                                    .CsvColumnsDelimiter;
                                                                                    }
                                                                                    x += y["ColumnName"].ToString();
                                                                                    j ++;
                                                                                    return
                                                                                            x;
                                                                                }
                                                                            );
                                            //await
                                            streamWriter
                                                    .WriteLine
                                                            (
                                                                columnsHeaderLine
                                                            );
                                            //streamWriter
                                            //        .Flush();
                                        }
                                    }
                                }
                                string line = string.Empty;
                                if (outputColumns == null)
                                {
                                    var fieldsCount = reader.FieldCount;
                                    for (var fieldIndex = 0; fieldIndex < fieldsCount; fieldIndex++)
                                    {
                                        if (fieldIndex > 0)
                                        {
                                            line += _csvFormatterOptions.CsvColumnsDelimiter;
                                        }
                                        line += GetFieldValue(reader, fieldIndex);
                                    }
                                }
                                else
                                {
                                    var j = 0;
                                    foreach (var (columnName, columnTitle, dataFormat) in outputColumns)
                                    {
                                        if (j > 0)
                                        {
                                            line += _csvFormatterOptions
                                                                .CsvColumnsDelimiter;
                                        }
                                        if
                                            (
                                                columns
                                                        .Any
                                                            (
                                                                (x) =>
                                                                {
                                                                    return
                                                                        (
                                                                            string
                                                                                .Compare
                                                                                    (
                                                                                        x["ColumnName"].ToString()
                                                                                        , columnName
                                                                                        , true
                                                                                    )
                                                                            ==
                                                                            0
                                                                        );
                                                                }
                                                            )
                                            )
                                        {
                                            var fieldIndex = reader.GetOrdinal(columnName);
                                            line += GetFieldValue(reader, fieldIndex, dataFormat);
                                        }
                                        j ++;
                                    }
                                }
                                //await
                                streamWriter
                                        //.WriteLineAsync(line);
                                        .WriteLine(line);
                                streamWriter
                                        //.FlushAsync();
                                        .Flush();
                                //i++;
                            }
                            , Request
                                    .Method
                            //, 102
                        );
                streamWriter.Close();
                streamWriter = null;
            }
        }
    }
}
#endif
