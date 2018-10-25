namespace Microshaoft
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    public static class SecureStringHelper
    {

        public static string ExtractToString(this SecureString secureString)
        {
            return
                new NetworkCredential
                        (
                            string.Empty
                            , secureString
                        ).Password;
        }
        public static string ExtractToString2(this SecureString target)
        {
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal
                                .SecureStringToGlobalAllocUnicode(target);
                return
                    Marshal
                        .PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
        public static string ExtractToString3(this SecureString target)
        {
            string plainString;
            IntPtr bstr = IntPtr.Zero;

            if (target == null || target.Length == 0)
                return string.Empty;

            try
            {
                bstr = Marshal.SecureStringToBSTR(target);
                plainString = Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                if (bstr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(bstr);
            }
            return plainString;
        }
        public static unsafe SecureString CreateSecureString(this string plainString)
        {
            SecureString secureString;

            if (plainString == null || plainString.Length == 0)
                return new SecureString();

            fixed (char* pch = plainString)
            {
                secureString = new SecureString(pch, plainString.Length);
            }

            return secureString;
        }
    }
}
