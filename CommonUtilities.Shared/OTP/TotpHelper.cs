﻿namespace Microshaoft
{
    using System;
    public static class TotpHelper
    {
        private static readonly Lazy<Totp> Totp = new Lazy<Totp>(() => new Totp());

        /// <summary>
        /// Generates code for the specified <paramref name="securityToken"/>.
        /// </summary>
        /// <param name="securityToken">The security token to generate code.</param>
        /// <param name="size">return  code size</param>
        /// <returns>The generated code.</returns>
        public static string GenerateCode(byte[] securityToken)
        {
            if (securityToken == null)
            {
                throw new ArgumentNullException(nameof(securityToken));
            }

            return Totp.Value.Compute(securityToken);
        }

        /// <summary>
        /// ttl of the code for the specified <paramref name="securityToken"/>.
        /// </summary>
        /// <param name="securityToken">The security token to generate code.</param>
        /// <param name="size">return  code size</param>
        /// <returns>the code remaining seconds expires in</returns>
        public static int TTL(byte[] securityToken)
        {
            if (securityToken == null)
            {
                throw new ArgumentNullException(nameof(securityToken));
            }

            return Totp.Value.RemainingSeconds();
        }

        /// <summary>
        /// Validates the code for the specified <paramref name="securityToken"/>.
        /// </summary>
        /// <param name="securityToken">The security token for verifying.</param>
        /// <param name="code">The code to validate.</param>
        /// <param name="expiresIn">expiresIn, in seconds</param>
        /// <returns><c>True</c> if validate succeed, otherwise, <c>false</c>.</returns>
        public static bool VerifyCode(byte[] securityToken, string code, int expiresIn = 30)
        {
            if (securityToken == null)
            {
                throw new ArgumentNullException(nameof(securityToken));
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }
            var validateResult = Totp.Value.Verify(securityToken, code, TimeSpan.FromSeconds(expiresIn));
            return validateResult;
        }

        /// <summary>
        /// Generates code for the specified <paramref name="securityToken"/>.
        /// </summary>
        /// <param name="securityToken">The security token to generate code.</param>
        /// <returns>The generated code.</returns>
        public static string GenerateCode(string securityToken) => GenerateCode(System.Text.Encoding.UTF8.GetBytes(securityToken));

        /// <summary>
        /// ttl of the code for the specified <paramref name="securityToken"/>.
        /// </summary>
        /// <param name="securityToken">The security token to generate code.</param>
        public static int TTL(string securityToken) => TTL(System.Text.Encoding.UTF8.GetBytes(securityToken));

        /// <summary>
        /// Validates the code for the specified <paramref name="securityToken"/>.
        /// </summary>
        /// <param name="securityToken">The security token for verifying.</param>
        /// <param name="code">The code to validate.</param>
        /// <param name="expiresIn">expiresIn, in seconds</param>
        /// <returns><c>True</c> if validate succeed, otherwise, <c>false</c>.</returns>
        public static bool VerifyCode(string securityToken, string code, int expiresIn = 30) => VerifyCode(System.Text.Encoding.UTF8.GetBytes(securityToken), code, expiresIn);
    }
}