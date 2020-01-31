// https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-3.1
namespace Microshaoft
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    public static class PasswordHasherHelper
    {
        /* =======================
        *  HASHED PASSWORD FORMATS
        *  =======================
        *
        *  Version 0:
        *  PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subKey, 1000 iterations.
        *  (See also: SDL crypto guidelines v5.1, Part III)
        *  Format: { 0x00, salt, subKey }
        */
        public static byte[] Hash
                                (
                                    string password
                                    , HashAlgorithmName hashAlgorithmName = default
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int subKeyBits = 256
                                )
        {
            byte[] processBytes(byte[] saltBytes, byte[] subKeyBytes)
            {
                var bytes = new byte
                                    [
                                        1
                                        + saltBytes.Length
                                        + subKeyBytes.Length
                                    ];
                var p = 1;
                var l = saltBytes.Length;
                Buffer
                    .BlockCopy
                        (
                            saltBytes
                            , 0
                            , bytes
                            , p
                            , l
                        );
                p += l;
                l = subKeyBytes.Length;
                Buffer
                    .BlockCopy
                        (
                            subKeyBytes
                            , 0
                            , bytes
                            , p
                            , l
                        );
                return
                    bytes;

            }
            if (hashAlgorithmName == default)
            {
                using
                    (
                        var rfc2898DeriveBytes = new Rfc2898DeriveBytes
                                                    (
                                                        password
                                                        , saltBits / 8
                                                        , iterationCount
                                                    )
                    )
                {
                    var saltBytes = rfc2898DeriveBytes.Salt;
                    var subKeyBytes = rfc2898DeriveBytes.GetBytes(subKeyBits / 8);
                    return
                        processBytes(saltBytes, subKeyBytes);
                }
            }
            else
            {
                using
                       (
                           var rfc2898DeriveBytes = new Rfc2898DeriveBytes
                                                       (
                                                           password
                                                           , saltBits / 8
                                                           , iterationCount
                                                           , hashAlgorithmName
                                                       )
                       )
                {
                    var saltBytes = rfc2898DeriveBytes.Salt;
                    var subKeyBytes = rfc2898DeriveBytes.GetBytes(subKeyBits / 8);
                    return
                        processBytes(saltBytes, subKeyBytes);
                }
            }
        }
        public static string HashToBase64String
                                (
                                    string password
                                    , HashAlgorithmName hashAlgorithmName = default
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int subKeyBits = 256
                                )
        {
            return
                Convert
                        .ToBase64String
                            (
                                Hash(password, hashAlgorithmName, saltBits, iterationCount, subKeyBits)
                            );
        }

        public static bool Verify
                                (
                                    string verifyingPassword
                                    , string base64HashedPassword
                                    , HashAlgorithmName hashAlgorithmName = default
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int subKeyBits = 256
                                )
        {
            return
                Verify
                    (
                        verifyingPassword
                        , Convert.FromBase64String(base64HashedPassword)
                        , hashAlgorithmName
                        , saltBits
                        , iterationCount
                        , subKeyBits
                    );
        }
        public static bool Verify
                                (
                                    string verifyingPassword
                                    , byte[] hashedPasswordBytes
                                    , HashAlgorithmName hashAlgorithmName
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int subKeyBits = 256
                                )
        {
            var p = 1;
            byte[] saltBytes = new byte[saltBits / 8];
            var l = saltBytes.Length;
            Buffer.BlockCopy(hashedPasswordBytes, p, saltBytes, 0, l);
            p += l;
            byte[] subKeyBytes = new byte[subKeyBits / 8];
            l = subKeyBytes.Length;
            Buffer.BlockCopy(hashedPasswordBytes, p, subKeyBytes, 0, l);
            if (hashAlgorithmName == default)
            {
                using
                    (
                        var rfc2898DeriveBytes = new Rfc2898DeriveBytes
                                        (
                                            verifyingPassword
                                            , saltBytes
                                            , iterationCount
                                        )
                    )
                {
                    return
                            rfc2898DeriveBytes
                                .GetBytes(subKeyBytes.Length)
                                .SequenceEqual(subKeyBytes);
                }
            }
            else
            {
                using
                        (
                            var rfc2898DeriveBytes = new Rfc2898DeriveBytes
                                            (
                                                verifyingPassword
                                                , saltBytes
                                                , iterationCount
                                                , hashAlgorithmName
                                            )
                        )
                {
                    return
                            rfc2898DeriveBytes
                                .GetBytes(subKeyBytes.Length)
                                .SequenceEqual(subKeyBytes);
                }
            }
        }
        static void Main(string[] args)
        {
            var password = "!@#123QWE";

            var hashedPassword = PasswordHasherHelper
                                        .HashToBase64String
                                            (password, HashAlgorithmName.SHA512);
            Console.WriteLine($"{nameof(password)} : {password}");
            Console.WriteLine($"{nameof(hashedPassword)} : {hashedPassword}");

            hashedPassword = PasswordHasherHelper
                                        .HashToBase64String
                                            (password, HashAlgorithmName.SHA512);

            Console.WriteLine($"{nameof(password)} : {password}");
            Console.WriteLine($"{nameof(hashedPassword)} : {hashedPassword}");

            var password2 = password;
            //password2 = "1111111";

            var r = PasswordHasherHelper
                                    .Verify
                                        (
                                            password2
                                            , hashedPassword
                                            , HashAlgorithmName.SHA512
                                        );
            Console.WriteLine(r);



            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
