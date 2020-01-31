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
        *  PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
        *  (See also: SDL crypto guidelines v5.1, Part III)
        *  Format: { 0x00, salt, subkey }
        */
        public static byte[] Hash
                                (
                                    string password
                                    , HashAlgorithmName hashAlgorithmName = default
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int keyBits = 256
                                )
        {
            byte[] processBytes(byte[] saltBytes, byte[] keyBytes)
            {
                var bytes = new byte
                                    [
                                        1
                                        + saltBytes.Length
                                        + keyBytes.Length
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
                l = keyBytes.Length;
                Buffer
                    .BlockCopy
                        (
                            keyBytes
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
                        var deriveBytes = new Rfc2898DeriveBytes
                                                    (
                                                        password
                                                        , saltBits / 8
                                                        , iterationCount
                                                    )
                    )
                {
                    var saltBytes = deriveBytes.Salt;
                    var keyBytes = deriveBytes.GetBytes(keyBits / 8);
                    return
                        processBytes(saltBytes, keyBytes);
                }
            }
            else
            {
                using
                       (
                           var deriveBytes = new Rfc2898DeriveBytes
                                                       (
                                                           password
                                                           , saltBits / 8
                                                           , iterationCount
                                                           , hashAlgorithmName
                                                       )
                       )
                {
                    var saltBytes = deriveBytes.Salt;
                    var keyBytes = deriveBytes.GetBytes(keyBits / 8);
                    return
                        processBytes(saltBytes, keyBytes);
                }
            }
        }
        public static string HashToBase64String
                                (
                                    string password
                                    , HashAlgorithmName hashAlgorithmName = default
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int keyBits = 256
                                )
        {
            return
                Convert
                        .ToBase64String
                            (
                                Hash(password, hashAlgorithmName, saltBits, iterationCount, keyBits)
                            );
        }

        public static bool Verify
                                (
                                    string verifyingPassword
                                    , string base64HashedPassword
                                    , HashAlgorithmName hashAlgorithmName = default
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int keyBits = 256
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
                        , keyBits
                    );
        }
        public static bool Verify
                                (
                                    string verifyingPassword
                                    , byte[] hashedPasswordBytes
                                    , HashAlgorithmName hashAlgorithmName
                                    , int saltBits = 128
                                    , int iterationCount = 1000
                                    , int keyBits = 256
                                )
        {
            var p = 1;
            byte[] saltBytes = new byte[saltBits / 8];
            var l = saltBytes.Length;
            Buffer.BlockCopy(hashedPasswordBytes, p, saltBytes, 0, l);
            p += l;
            byte[] keyBytes = new byte[keyBits / 8];
            l = keyBytes.Length;
            Buffer.BlockCopy(hashedPasswordBytes, p, keyBytes, 0, l);
            if (hashAlgorithmName == default)
            {
                using
                    (
                        var r = new Rfc2898DeriveBytes
                                        (
                                            verifyingPassword
                                            , saltBytes
                                            , iterationCount
                                        )
                    )
                {
                    return
                            r
                                .GetBytes(keyBytes.Length)
                                .SequenceEqual(keyBytes);
                }
            }
            else
            {
                using
                        (
                            var r = new Rfc2898DeriveBytes
                                            (
                                                verifyingPassword
                                                , saltBytes
                                                , iterationCount
                                                , hashAlgorithmName
                                            )
                        )
                {
                    return
                            r
                                .GetBytes(keyBytes.Length)
                                .SequenceEqual(keyBytes);
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
