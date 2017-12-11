namespace Microshaoft
{
    using System;
    public static class DebugOutputConsole
    {
        //private static TextWriter _out = Console.Out;

        //
        // Summary:
        //     Writes the text representation of the specified 64-bit signed integer value to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc,long value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the specified string value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, string value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified 64-bit unsigned integer value
        //     to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        [CLSCompliant(false)]
        public static void WriteIf(Func<bool> onPredicateProcessFunc, ulong value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified 32-bit unsigned integer value
        //     to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //[CLSCompliant(false)]
        public static void WriteIf(Func<bool> onPredicateProcessFunc, uint value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified object to the standard output
        //     stream.
        //
        // Parameters:
        //   value:
        //     The value to write, or null.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, object value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified single-precision floating-point
        //     value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, float value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified System.Decimal value to the standard
        //     output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, decimal value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified double-precision floating-point
        //     value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, double value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the specified array of Unicode characters to the standard output stream.
        //
        // Parameters:
        //   buffer:
        //     A Unicode character array.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, char[] buffer)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(buffer);
        }
        //
        // Summary:
        //     Writes the specified Unicode character value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, char value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified Boolean value to the standard
        //     output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, bool value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified 32-bit signed integer value to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, int value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified object to the standard output
        //     stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg0:
        //     An object to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, string format, object arg0)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(format, arg0);
        }
        //
        // Summary:
        //     Writes the text representation of the specified array of objects to the standard
        //     output stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format or arg is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, string format, params object[] arg)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(format, arg);
        }
        //
        // Summary:
        //     Writes the text representation of the specified objects to the standard output
        //     stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, string format, object arg0, object arg1)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(format, arg0, arg1);
        }
        //
        // Summary:
        //     Writes the specified subarray of Unicode characters to the standard output stream.
        //
        // Parameters:
        //   buffer:
        //     An array of Unicode characters.
        //
        //   index:
        //     The starting position in buffer.
        //
        //   count:
        //     The number of characters to write.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     index or count is less than zero.
        //
        //   T:System.ArgumentException:
        //     index plus count specify a position that is not within buffer.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, char[] buffer, int index, int count)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(buffer, index, count);
        }
        //
        // Summary:
        //     Writes the text representation of the specified objects to the standard output
        //     stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        //   arg2:
        //     The third object to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteIf(Func<bool> onPredicateProcessFunc, string format, object arg0, object arg1, object arg2)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(format, arg0, arg1, arg2);
        }
        [CLSCompliant(false)]
        public static void Write(Func<bool> onPredicateProcessFunc, string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.Write(format, arg0, arg1, arg2, arg3);
        }
        //
        // Summary:
        //     Writes the current line terminator to the standard output stream.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine();
        }
        //
        // Summary:
        //     Writes the text representation of the specified Boolean value, followed by the
        //     current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, bool value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified single-precision floating-point
        //     value, followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, float value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified 32-bit signed integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, int value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified 32-bit unsigned integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        [CLSCompliant(false)]
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, uint value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified 64-bit signed integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, long value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified 64-bit unsigned integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        [CLSCompliant(false)]
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, ulong value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified object, followed by the current
        //     line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, object value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the specified string value, followed by the current line terminator, to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, string value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified double-precision floating-point
        //     value, followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, double value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified System.Decimal value, followed
        //     by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, decimal value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the specified array of Unicode characters, followed by the current line
        //     terminator, to the standard output stream.
        //
        // Parameters:
        //   buffer:
        //     A Unicode character array.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, char[] buffer)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(buffer);
        }
        //
        // Summary:
        //     Writes the specified Unicode character, followed by the current line terminator,
        //     value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, char value)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(value);
        }
        //
        // Summary:
        //     Writes the text representation of the specified array of objects, followed by
        //     the current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format or arg is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, string format, params object[] arg)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(format, arg);
        }

        //
        // Summary:
        //     Writes the text representation of the specified object, followed by the current
        //     line terminator, to the standard output stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg0:
        //     An object to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, string format, object arg0)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(format, arg0);
        }
        //
        // Summary:
        //     Writes the text representation of the specified objects, followed by the current
        //     line terminator, to the standard output stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, string format, object arg0, object arg1)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(format, arg0, arg1);
        }
        //
        // Summary:
        //     Writes the specified subarray of Unicode characters, followed by the current
        //     line terminator, to the standard output stream.
        //
        // Parameters:
        //   buffer:
        //     An array of Unicode characters.
        //
        //   index:
        //     The starting position in buffer.
        //
        //   count:
        //     The number of characters to write.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     index or count is less than zero.
        //
        //   T:System.ArgumentException:
        //     index plus count specify a position that is not within buffer.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, char[] buffer, int index, int count)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(buffer, index, count);
        }
        //
        // Summary:
        //     Writes the text representation of the specified objects, followed by the current
        //     line terminator, to the standard output stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string (see Remarks).
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        //   arg2:
        //     The third object to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, string format, object arg0, object arg1, object arg2)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        [CLSCompliant(false)]
        public static void WriteLineIf(Func<bool> onPredicateProcessFunc, string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (!onPredicateProcessFunc())
            {
                return;
            }
            Console.WriteLine(format, arg0, arg1, arg2, arg3);
        }

    }
}
