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
    using System.Data.SqlClient;
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
 
        private string GetFieldValue
                            (
                                IDataReader reader
                                , int fieldIndex
                                , string format = null
                                , string digitsTextSuffix = null
                            )
        {
            string @value = string.Empty;
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
                    string
                        .Compare
                            (
                                reader
                                    .GetDataTypeName(fieldIndex)
                                , "Time"
                                , true
                            )
                    ==
                    0
                )
            {
                TimeSpan? timeSpan = null;
                if (reader is SqlDataReader sqlDataReader)
                {
                    timeSpan = sqlDataReader.GetTimeSpan(fieldIndex);
                }
                if (timeSpan != null)
                {
                    if (format.IsNullOrEmptyOrWhiteSpace())
                    {
                        format = _csvFormatterOptions.TimeFormat;
                    }
                    if (!format.IsNullOrEmptyOrWhiteSpace())
                    {
                        @value = $@"""{timeSpan.Value.ToString(format)}""";
                    }
                    else
                    {
                        @value = $@"""{timeSpan.Value.ToString()}""";
                    }
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
                @value = reader
                            .GetValue(fieldIndex)
                            .ToString();
                if (fieldType == typeof(string))
                {
                    if
                        (
                            digitsTextSuffix == null
                        )
                    {
                        digitsTextSuffix = _csvFormatterOptions
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
                        //避免在Excel中csv文本数字自动变科学计数法
                        if
                            (
                                (
                                    @value
                                        .Length
                                    >
                                    _csvFormatterOptions
                                        .MinExclusiveLengthDigitsTextSuffix
                                )
                                ||
                                @value
                                    .StartsWith("0")
                            )
                        {
                            @value += digitsTextSuffix;
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
                                                _csvFormatterOptions
                                                        .CsvColumnsDelimiter
                                            )
                                ||
                                @value
                                    .Contains("\r")
                                ||
                                @value
                                    .Contains("\n")
                            )
                        {
                            @value = $@"""{@value}""";
                        }
                    }
                }
            }
            return
                @value;
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
                     "export11111/{routeName}/"
                 )
        ]
        [OperationsAuthorizeFilter(false)]
        [
            RequestJTokenParametersProcessFilter
                    (
                        AccessingConfigurationKey = "DefaultAccessing"
                    )
        ]
        public virtual async Task
                             ProcessActionRequestAsync
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
            if 
                (
                    _configuration
                                .TryGetSection
                                    (
                                        $"Routes:{routeName}:{httpMethod}:Exporting:DownloadFileName"
                                        , out var downloadFileNameConfiguration
                                    )
                )
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

                (
                    string ColumnName
                    , string ColumnTitle
                    , string DataFormat
                    , string DigitsTextSuffix
                )
                    [][] allOutputColumns = null;
                if 
                    (
                        _configuration
                                    .TryGetSection
                                        (
                                            $"Routes:{routeName}:{httpMethod}:Exporting:OutputColumns"
                                            , out var allOutputColumnsConfiguration
                                        )
                    )
                {
                    allOutputColumns = allOutputColumnsConfiguration
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
                                                                        var columnName =
                                                                                    xx
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
                }
                await
                    _service
                        .ProcessReaderReadRowsAsync
                            (
                                routeName
                                , parameters
                                , async (resultSetIndex, reader, columns, rowIndex) =>
                                {
                                    if (rowIndex == 0)
                                    {
                                        if (_csvFormatterOptions.UseSingleLineHeaderInCsv)
                                        {
                                            if 
                                                (
                                                    allOutputColumns == null
                                                    ||
                                                    resultSetIndex >= allOutputColumns.Length
                                                )
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
                                                //streamWriter
                                                //        .Flush();
                                            }
                                            else
                                            {
                                                if (resultSetIndex < allOutputColumns.Length)
                                                {
                                                    var j = 0;
                                                    (
                                                        string ColumnName
                                                        , string ColumnTitle
                                                        , string DataFormat
                                                        , string DigitsTextSuffix
                                                    )
                                                        [] outputColumns = allOutputColumns[resultSetIndex];
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
                                        }
                                    }
                                    string line = string.Empty;
                                    if 
                                        (
                                            allOutputColumns != null
                                            &&
                                            resultSetIndex < allOutputColumns.Length
                                        )
                                    {
                                        (
                                            string ColumnName
                                            , string ColumnTitle
                                            , string DataFormat
                                            , string DigitsTextSuffix
                                        )
                                            [] outputColumns = allOutputColumns[resultSetIndex];
                                        var j = 0;
                                        foreach (var (columnName, columnTitle, dataFormat, digitsTextSuffix) in outputColumns)
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
                                            j++;
                                        }
                                    }
                                    else
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
                                    await
                                        streamWriter
                                                .WriteLineAsync(line);
                                                //.WriteLine(line);
                                    await
                                        streamWriter
                                                .FlushAsync();
                                                //.Flush();
                                    //i++;
                                }
                                , Request
                                        .Method
                                //, 102
                            );
                await
                    streamWriter
                            .FlushAsync();
                streamWriter.Close();
            }
        }
    }
}
#endif
