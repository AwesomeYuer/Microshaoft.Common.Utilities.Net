namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public static class NumberHelper
    {
        public static bool TryParse<T>(string text, out T result)
        {
            var r = false;
            result = default(T);
            if (typeof(T) == typeof(int))
            {
                var typeCode = Type.GetTypeCode(typeof(int));
                r = int.TryParse(text, out var rr);
                if (r)
                {
                    result = (T) Convert.ChangeType(rr, typeCode);
                }
            }

            return r;
        }


    }
}
