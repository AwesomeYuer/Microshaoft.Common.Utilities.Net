﻿namespace Microshaoft
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
            object @return = null;
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
                @return = DBNull.Value;
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
                    @return = jValueText;
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
                    if
                        (
                            DateTime
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Boolean
                    )
                {
                    if
                        (
                            bool
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Decimal
                    )
                {
                    if
                        (
                            decimal
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
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
                                        , out var @value
                                    );
                    if (b)
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Raw
                    )
                {
                    if
                        (
                            Guid
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Long
                        ||
                        target.OracleDbType == OracleDbType.Int64
                    )
                {
                    if
                        (
                            long
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Int32
                    )
                {
                    if
                        (
                            int
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.OracleDbType == OracleDbType.Int16
                    )
                {
                    if
                        (
                            short
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
            }
            return @return;
        }
    }
}
