namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using Oracle.ManagedDataAccess.Client;
    public static partial class OracleDbParameterHelper
    {
        public static object SetGetValueAsObject
                                    (
                                        this OracleParameter target
                                        , JToken jValue 
                                    )
        {
            object r = null;
            if
                (
                    jValue == null
                    ||
                    jValue.Type == JTokenType.Null
                    ||
                    jValue.Type == JTokenType.Undefined
                    ||
                    jValue.Type == JTokenType.None
                )
            {
                r = DBNull.Value;
            }
            else
            {
                var jValueText = jValue.ToString();
                if
                    (
                        target.OracleDbType == OracleDbType.Varchar2
                        ||
                        target.OracleDbType == OracleDbType.NVarchar2
                        ||
                        target.OracleDbType == OracleDbType.Char
                        ||
                        target.OracleDbType == OracleDbType.NChar
                    )
                {
                    r = jValueText;
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Date
                        ||
                        target.OracleDbType == OracleDbType.TimeStamp
                        ||
                        target.OracleDbType == OracleDbType.TimeStampLTZ
                        ||
                        target.OracleDbType == OracleDbType.TimeStampTZ
                    )
                {
                    var b = DateTime
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Boolean
                    )
                {
                    var b = bool
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Decimal
                    )
                {
                    var b = decimal
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Double
                    )
                {
                    var b = double
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Raw
                    )
                {
                    var b = Guid
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Long
                        ||
                        target.OracleDbType == OracleDbType.Int64
                    )
                {
                    var b = long
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Int32
                    )
                {
                    var b = int
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Int16
                    )
                {
                    var b = short
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }


            }
            return r;
        }
    }
}
