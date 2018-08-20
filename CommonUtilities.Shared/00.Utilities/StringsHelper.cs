namespace Microshaoft
{
    using System;
    using System.Text;
    public static class StringHelper
    {
        public static int FindIndex
                                (
                                    this string target
                                    , string search
                                    , int hits
                                    , bool ignoreCase = true
                                )
        {
            int r = -1;
            int p = 0;
            var s = target;
            var ss = search;
            if (ignoreCase)
            {
                s = target.ToUpper();
                ss = search.ToUpper();
            }
            var i = 0;
            var l = target.Length;
            var ll = search.Length;
            while (p <= l && p >= 0)
            {
                p = s.IndexOf(ss, p + ll);
                if (p > 0)
                {
                    i++;
                }
                if (i >= hits)
                {
                    break;
                }
            }
            return p;
        }


        public static bool IsNullOrEmptyOrWhiteSpace(this string target)
        {
            return 
                    string.IsNullOrEmpty(target)
                    || string.IsNullOrWhiteSpace(target);
        }
        public static bool IsValidString(string text)
        {
            return (text != string.Empty && text != null && text.Trim().Length > 0);
        }
        public static string PadLeftInBytes
                            (
                                this string text
                                , int totalWidth
                                , char paddingChar = ' '
                                , Encoding encoding = null
                            )
        {
            if (encoding == null)
            {
                encoding = Encoding.GetEncoding("gb2312");
            }
            totalWidth -=
                            (
                                encoding.GetByteCount(text)
                                - text.Length
                            );
            return
                text
                    .PadLeft
                        (
                            totalWidth
                            , paddingChar
                        );
        }
        public static string PadRightInBytes
                            (
                                this string text
                                , int totalWidth
                                , char paddingChar = ' '
                                , Encoding encoding = null
                            )
        {
            if (encoding == null)
            {
                encoding = Encoding.GetEncoding("gb2312");
            }
            totalWidth -=
                            (
                                encoding.GetByteCount(text)
                                - text.Length
                            );
            return
                text
                    .PadRight
                        (
                            totalWidth
                            , paddingChar
                        );
        }
    }
    public static class StringsHelper
    {
        public static bool StringsCompareWithWild(Tuple<string, string>[] a, string wild = "*")
        {
            var r = true;
            foreach (var xx in a)
            {
                r = r &&
                    (
                        xx.Item1 == wild ?
                        true
                        :
                        (string.Compare(xx.Item1, xx.Item2, true) == 0)
                    );
                if (!r)
                {
                    break;
                }
            }
            return r;
        }
    }
}
namespace Microshaoft
{
    using System;
    using System.Collections.Specialized;
    using System.Text;

    public static class HttpHelper
    {
        public static string GenerateFormHTML
                                (
                                    NameValueCollection httpRequestFields
                                    , string formName
                                    , string httpMethod
                                    , string actionUrl
                                )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\"><br>", formName, httpMethod, actionUrl));
            foreach (string var in httpRequestFields.AllKeys)
            {
                string s = string.Format
                                    (
                                        "{0} <input type=\"text\" name=\"{0}\" value=\"{1}\" /><br>"
                                        , var
                                        , httpRequestFields[var]
                                    );
                if (StringHelper.IsValidString(s))
                {
                    sb.Append(s);
                }
            }
            sb.Append("<input type=\"submit\" />");
            sb.Append("</form>");
            //sb.Append(string.Format("<script type=\"text/javascript\">alert('asda');{0}.submit()</script>", formName));
            return sb.ToString();
        }
    }
}


