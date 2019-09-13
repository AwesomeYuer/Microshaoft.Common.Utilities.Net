namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    public static class LongIntegerHelper
    {
        public readonly static string[] DigitsChars
                    =       "0123456789ABCDEFGHJKLMNPQRTUVWXY"  //不许删
                        //  "BU2EFV7MXY9CD01JK3A5LG4HNW6RTP8Q"  //不许删
                            .ToCharArray()
                            .Select
                                (
                                    (x) =>
                                    {
                                        return
                                            x
                                                .ToString()
                                                .Trim();
                                    }
                                )
                            .ToArray();
        public readonly static Dictionary<string, int> CharsDigits
                    = new Func<Dictionary<string, int>>
                            (
                                () =>
                                {
                                    int i = 0;
                                    return
                                        DigitsChars
                                            .ToDictionary
                                                (
                                                    (x) =>
                                                    {
                                                        return x;
                                                    }
                                                    ,
                                                    (x) =>
                                                    {
                                                        return i++;
                                                    }
                                                );
                                }
                             )();
        public static string ConvertDecimalToBase32String(ulong x)
        {
            ulong diviend = x;
            uint divisor = (uint) DigitsChars.Length;
            ulong quotient = 0;
            ulong remainder = 0;
            int digits = 0;
            var digitChar = string.Empty;
            var result = string.Empty;
            do
            {
                quotient = diviend / divisor;
                remainder = diviend % divisor;
                digitChar = DigitsChars[remainder];
                result = string.Format("{0}{1}", digitChar, result);
                diviend = quotient;
                digits++;
            } while (quotient >= divisor);
            if (quotient > 0)
            {
                //digitChar = DigitsChars[(int) quotient];
                //不能强制转换???????
                digitChar = DigitsChars[quotient];
                result = string.Format("{0}{1}", digitChar, result);
            }
            return result;
        }
        public static ulong ConvertBase32StringToDecimal(string number32)
        {
            var digits = number32
                            .ToCharArray()
                            .Select
                                (
                                    (x) =>
                                    {
                                        return
                                            CharsDigits[x.ToString()];
                                    }
                                )
                            .Reverse()
                            .ToArray();
            ulong r = 0;
            int i = 0;
            foreach (var x in digits)
            {
                r += ((ulong) x * (ulong) BigInteger.Pow(32, i));
                i++;
            }
            return r;
        }
    }
}
