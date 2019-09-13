#pragma warning disable SCS0006
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Text;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    public class Secret
    {
        public byte[] EncryptorSharedEncryptedOnceKey;
        public byte[] EncryptorSharedEncryptedOnceIV;
        public byte[] EncryptorHashSignature;
        public byte[] EncryptorPublicKeyCerRawData;
        public byte[] EncryptedData;
        public HashSignatureMode SignHashMode;
        public bool DoOAEPadding;
    }
    public enum HashSignatureMode
    {
        MD5
        , SHA1
    }
    public static class CryptographyHelper
    {
        public static byte[] HybridDecrypt
                                    (
                                        X509Certificate2 decryptorPrivateKeyPfx
                                        , Secret data
                                    )
        {
            X509Certificate2 encryptorPublicKeyCer = null;
            try
            {
                RSACryptoServiceProvider decryptorPrivateKeyPfxProvider = decryptorPrivateKeyPfx.PrivateKey as RSACryptoServiceProvider;
                encryptorPublicKeyCer = new X509Certificate2(data.EncryptorPublicKeyCerRawData);
                RSACryptoServiceProvider encryptorPublicKeyCerProvider = encryptorPublicKeyCer.PublicKey.Key as RSACryptoServiceProvider;
                return HybridDecrypt
                                    (
                                        decryptorPrivateKeyPfxProvider
                                        , encryptorPublicKeyCerProvider
                                        , data
                                    );
            }
            catch
            {
                return null;
            }
            finally
            {
                if (encryptorPublicKeyCer != null)
                {
                    encryptorPublicKeyCer.Reset();
                }
            }
        }
        public static byte[] HybridDecrypt
                                    (
                                        RSACryptoServiceProvider decryptorPrivateKeyPfxProvider
                                        , RSACryptoServiceProvider encryptorPublicKeyCerProvider
                                        , Secret data
                                    )
        {
            byte[] buffer = null;
            HashAlgorithm hashAlgorithm;
            if (data.SignHashMode == HashSignatureMode.SHA1)
            {
                hashAlgorithm = new SHA1CryptoServiceProvider();
            }
            else //(hashSignatureMode == HashSignatureMode.MD5)
            {
                hashAlgorithm = new MD5CryptoServiceProvider();
            }
            using (MemoryStream stream = new MemoryStream())
            {
                buffer = data.EncryptorSharedEncryptedOnceIV;
                stream.Write(buffer, 0, buffer.Length);
                buffer = data.EncryptorSharedEncryptedOnceKey;
                stream.Write(buffer, 0, buffer.Length);
                buffer = data.EncryptedData;
                stream.Position = 0;
                buffer = hashAlgorithm.ComputeHash(stream);
                stream.Close();
            }
            //X509Certificate2 encryptorPublicKeyCer = new X509Certificate2(data.EncryptorPublicKeyCerRawData);
            //RSACryptoServiceProvider encryptorPublicKeyCerProvider = encryptorPublicKeyCer.PublicKey.Key as RSACryptoServiceProvider;
            if (encryptorPublicKeyCerProvider.VerifyHash
                                                (
                                                    buffer
                                                    , Enum.GetName
                                                                (
                                                                    data.SignHashMode.GetType()
                                                                    , data.SignHashMode
                                                                )
                                                    , data.EncryptorHashSignature
                                                )
                )
            {
                //decryptorPrivateKeyPfxProvider = decryptorPrivateKeyPfx.PrivateKey as RSACryptoServiceProvider;
                using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
                {
                    buffer = data.EncryptorSharedEncryptedOnceIV;
                    buffer = decryptorPrivateKeyPfxProvider.Decrypt(buffer, data.DoOAEPadding);
                    des.IV = buffer;
                    buffer = data.EncryptorSharedEncryptedOnceKey;
                    buffer = decryptorPrivateKeyPfxProvider.Decrypt(buffer, data.DoOAEPadding);
                    des.Key = buffer;
                    buffer = data.EncryptedData;
                    buffer = des.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length);
                }
            }
            else
            {
                buffer = null;
            }
            return buffer;
        }
        public static Secret HybridEncrypt
                                    (
                                        byte[] encryptorPrivateKeyPfxRawData
                                        , byte[] encryptorPublicKeyCerRawData
                                        , byte[] decryptorPublicKeyCerRawData
                                        , HashSignatureMode hashSignatureMode
                                        , bool DoOAEPadding
                                        , byte[] data
                                    )
        {
            X509Certificate2 encryptorPrivateKeyPfx = null;
            X509Certificate2 encryptorPublicKeyCer = null;
            X509Certificate2 decryptorPublicKeyCer = null;
            try
            {
                encryptorPrivateKeyPfx = null;
                encryptorPublicKeyCer = null;
                decryptorPublicKeyCer = null;
                return HybridEncrypt
                        (
                            encryptorPrivateKeyPfx
                            , encryptorPublicKeyCer
                            , decryptorPublicKeyCer
                            , hashSignatureMode
                            , DoOAEPadding
                            , data
                        );
            }
            catch
            {
                return null;
            }
            finally
            {
                if (encryptorPrivateKeyPfx != null)
                {
                    encryptorPrivateKeyPfx.Reset();
                }
                if (encryptorPublicKeyCer != null)
                {
                    encryptorPublicKeyCer.Reset();
                }
                if (decryptorPublicKeyCer != null)
                {
                    decryptorPublicKeyCer.Reset();
                }
            }
        }
        public static Secret HybridEncrypt
                                    (
                                        string encryptorPrivateKeyPfxFileName
                                        , string encryptorPublicKeyCerFileName
                                        , string decryptorPublicKeyCerFileName
                                        , HashSignatureMode hashSignatureMode
                                        , bool DoOAEPadding
                                        , byte[] data
                                    )
        {
            X509Certificate2 encryptorPrivateKeyPfx = null;
            X509Certificate2 encryptorPublicKeyCer = null;
            X509Certificate2 decryptorPublicKeyCer = null;
            try
            {
                encryptorPrivateKeyPfx = new X509Certificate2(encryptorPrivateKeyPfxFileName);
                encryptorPublicKeyCer = new X509Certificate2(encryptorPublicKeyCerFileName);
                decryptorPublicKeyCer = new X509Certificate2(decryptorPublicKeyCerFileName);
                return HybridEncrypt
                        (
                            encryptorPrivateKeyPfx
                            , encryptorPublicKeyCer
                            , decryptorPublicKeyCer
                            , hashSignatureMode
                            , DoOAEPadding
                            , data
                        );
            }
            catch
            {
                return null;
            }
            finally
            {
                if (encryptorPrivateKeyPfx != null)
                {
                    encryptorPrivateKeyPfx.Reset();
                }
                if (encryptorPublicKeyCer != null)
                {
                    encryptorPublicKeyCer.Reset();
                }
                if (decryptorPublicKeyCer != null)
                {
                    decryptorPublicKeyCer.Reset();
                }
            }
        }
        public static Secret HybridEncrypt
                                    (
                                        X509Certificate2 encryptorPrivateKeyPfx
                                        , X509Certificate2 encryptorPublicKeyCer
                                        , X509Certificate2 decryptorPublicKeyCer
                                        , HashSignatureMode signHashMode
                                        , bool DoOAEPadding
                                        , byte[] data
                                    )
        {
            RSACryptoServiceProvider encryptorPrivateKeyPfxProvider = encryptorPrivateKeyPfx.PrivateKey as RSACryptoServiceProvider;
            RSACryptoServiceProvider decryptorPublicKeyCerProvider = decryptorPublicKeyCer.PublicKey.Key as RSACryptoServiceProvider;
            return HybridEncrypt
                        (
                            encryptorPrivateKeyPfxProvider
                            , encryptorPublicKeyCer
                            , decryptorPublicKeyCerProvider
                            , signHashMode
                            , DoOAEPadding
                            , data
                        );
        }
        public static Secret HybridEncrypt
                                    (
                                        RSACryptoServiceProvider encryptorPrivateKeyPfxProvider
                                        , X509Certificate2 encryptorPublicKeyCer
                                        , RSACryptoServiceProvider decryptorPublicKeyCerProvider
                                        , HashSignatureMode signHashMode
                                        , bool DoOAEPadding
                                        , byte[] data
                                    )
        {
            Secret secret = new Secret();
            using (TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider())
            {
                provider.GenerateIV();
                secret.EncryptorSharedEncryptedOnceIV = provider.IV;
                provider.GenerateKey();
                secret.EncryptorSharedEncryptedOnceKey = provider.Key;
                secret.EncryptedData = provider.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
            }
            secret.EncryptorSharedEncryptedOnceIV = decryptorPublicKeyCerProvider.Encrypt(secret.EncryptorSharedEncryptedOnceIV, DoOAEPadding);
            secret.EncryptorSharedEncryptedOnceKey = decryptorPublicKeyCerProvider.Encrypt(secret.EncryptorSharedEncryptedOnceKey, DoOAEPadding);
            HashAlgorithm hashAlgorithm;
            if (signHashMode == HashSignatureMode.SHA1)
            {

                hashAlgorithm = new SHA1CryptoServiceProvider();
            }
            else //(hashSignatureMode == HashSignatureMode.MD5)
            {
                hashAlgorithm = new MD5CryptoServiceProvider();
            }
            MemoryStream stream = new MemoryStream();
            byte[] buffer = secret.EncryptorSharedEncryptedOnceIV;
            stream.Write(buffer, 0, buffer.Length);
            buffer = secret.EncryptorSharedEncryptedOnceKey;
            stream.Write(buffer, 0, buffer.Length);
            buffer = secret.EncryptedData;
            stream.Position = 0;
            buffer = hashAlgorithm.ComputeHash(stream);
            stream.Close();
            stream.Dispose();
            secret.EncryptorHashSignature 
                        = encryptorPrivateKeyPfxProvider
                                                    .SignHash
                                                        (
                                                            buffer
                                                            , Enum.GetName
                                                                        (
                                                                            signHashMode.GetType()
                                                                            , signHashMode
                                                                        )
                                                        );
            secret.EncryptorPublicKeyCerRawData = encryptorPublicKeyCer.RawData;
            secret.SignHashMode = signHashMode;
            secret.DoOAEPadding = DoOAEPadding;
            return secret;
        }


        public static string GenerateTripleDESHexStringKey()
        {
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.GenerateKey();
            return BytesArrayToHexString(des.Key);
        }
        public static string GenerateTripleDESHexStringIV()
        {
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.GenerateIV();
            return BytesArrayToHexString(des.IV);
        }
        public static byte[] SymmetricAlgorithmEncrypt
                                (
                                     SymmetricAlgorithm algorithm
                                     , byte[] data
                                 )
        {
            return algorithm
                        .CreateEncryptor()
                        .TransformFinalBlock(data, 0, data.Length);
        }
        public static byte[] SymmetricAlgorithmEncrypt
                                       (
                                            SymmetricAlgorithm algorithm
                                            , string text
                                            , Encoding e
                                        )
        {
            return SymmetricAlgorithmEncrypt
                            (
                                algorithm
                                , e.GetBytes(text)
                            );
        }
        public static byte[] SymmetricAlgorithmDecrypt
                                        (
                                            SymmetricAlgorithm algorithm
                                            , byte[] data
                                         )
        {
            return algorithm
                        .CreateDecryptor()
                        .TransformFinalBlock(data, 0, data.Length);
        }
        public static string SymmetricAlgorithmDecrypt
                                        (
                                            SymmetricAlgorithm algorithm
                                            , byte[] data
                                            , Encoding e //原文的encoding
                                        )
        {
            return e.GetString
                        (
                            SymmetricAlgorithmDecrypt
                                                (
                                                    algorithm
                                                    , data
                                                )
                        );
        }
        public static byte[] ComputeHash
                                    (
                                        HashAlgorithm algorithm
                                        , byte[] data
                                    )
        {
            return algorithm.ComputeHash(data);
        }
        public static byte[] ComputeHash
                                    (
                                        HashAlgorithm algorithm
                                        , string text
                                        , Encoding e
                                    )
        {
            return ComputeHash(algorithm, e.GetBytes(text));
        }
        public static byte[] ComputeKeyedHash
                                    (
                                        KeyedHashAlgorithm algorithm
                                        , byte[] data
                                    )
        {
            return ComputeHash(algorithm, data);
        }
        public static byte[] ComputeKeyedHash
                                    (
                                        KeyedHashAlgorithm algorithm
                                        , string text
                                        , Encoding e
                                    )
        {
            return ComputeHash(algorithm, text, e);
        }
        public static byte[] RSASignSHA1
                        (
                            string privateKeyXml
                            , byte[] data
                        )
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(privateKeyXml);
            return RSASignSHA1
                        (
                            provider
                            , data
                        );
        }
        public static byte[] RSASignSHA1
                                (
                                    RSACryptoServiceProvider provider
                                    , byte[] data
                                )
        {
            using (var hashAlgorithm = new SHA1CryptoServiceProvider())
            {
                return
                    provider
                        .SignHash
                                (
                                    ComputeHash(hashAlgorithm, data)
                                    , "SHA1"
                                );
            }
        }
        public static bool RSAVerifySHA1
                                (
                                    string publicKeyXml
                                    , byte[] data
                                    , byte[] signature
                                )
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider
                    .FromXmlString(publicKeyXml);
                return
                    RSAVerifySHA1
                                (
                                    provider
                                    , data
                                    , signature
                                );
            }
        }
        public static bool RSAVerifySHA1
                                (
                                    RSACryptoServiceProvider provider
                                    , byte[] data
                                    , byte[] signature
                                )
        {
            using (HashAlgorithm hashAlgorithm = new SHA1CryptoServiceProvider())
            {
                return provider
                            .VerifyHash
                                (
                                    ComputeHash(hashAlgorithm, data)
                                    , "SHA1"
                                    , signature
                                );
            }
        }
        public static byte[] RSASignMD5
                                (
                                    string privateKeyXml
                                    , byte[] data
                                )
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider
                    .FromXmlString(privateKeyXml);
                return
                    RSASignMD5
                            (
                                provider
                                , data
                            );
            }
        }
        public static byte[] RSASignMD5
                                (
                                    RSACryptoServiceProvider provider
                                    , byte[] data
                                )
        {
            using (HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider())
            {
                return
                    provider
                        .SignHash
                                (
                                    ComputeHash(hashAlgorithm, data)
                                    , "MD5"
                                );
            }
        }
        public static bool RSAVerifyMD5
                                (
                                    string publicKeyXml
                                    , byte[] data
                                    , byte[] signature
                                )
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider
                    .FromXmlString(publicKeyXml);
                return
                    RSAVerifyMD5
                            (
                                provider
                                , data
                                , signature
                            );
            }
        }
        public static bool RSAVerifyMD5
                                (
                                    RSACryptoServiceProvider provider
                                    , byte[] data
                                    , byte[] signature
                                )
        {
            using (HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider())
            {
                return
                    provider
                        .VerifyHash
                                (
                                    ComputeHash(hashAlgorithm, data)
                                    , "MD5"
                                    , signature
                                );
            }
        }
        public static byte[] RSAEncrypt
                                (
                                    string publicKeyXml
                                    , byte[] data
                                    , bool DoOAEPPadding
                                )
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider
                    .FromXmlString(publicKeyXml);
                return
                    RSAEncrypt
                            (
                                provider
                                , data
                                , DoOAEPPadding
                            );
            }
        }
        public static byte[] RSAEncrypt
                        (
                            RSACryptoServiceProvider provider
                            , byte[] data
                            , bool DoOAEPPadding
                        )
        {
            return provider.Encrypt(data, DoOAEPPadding);
        }
        public static byte[] RSADecrypt
                                (
                                    string privateKeyXml
                                    , byte[] data
                                    , bool DoOAEPPadding
                                )
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider
                    .FromXmlString(privateKeyXml);
                return
                    RSADecrypt
                            (
                                provider
                                , data
                                , DoOAEPPadding
                            );
            }
        }
        public static byte[] RSADecrypt
                                (
                                    RSACryptoServiceProvider provider
                                    , byte[] data
                                    , bool DoOAEPPadding
                                )
        {
            return provider.Decrypt(data, DoOAEPPadding);
        }
        public static byte[] X509CertificateEncrypt
                                    (
                                        X509Certificate2 publicKeyCer
                                        , byte[] data
                                        , bool DoOAEPadding
                                    )
        {
            using (RSACryptoServiceProvider provider = publicKeyCer.PublicKey.Key as RSACryptoServiceProvider)
            {
                return
                    RSAEncrypt
                            (
                                provider
                                , data
                                , DoOAEPadding
                            );
            }
        }
        public static byte[] X509CertificateDecrypt
                                    (
                                        X509Certificate2 privateKeyPfx
                                        , byte[] data
                                        , bool DoOAEPadding
                                    )
        {
            using (var provider = privateKeyPfx.PrivateKey as RSACryptoServiceProvider)
            {
                return RSADecrypt
                            (
                                provider
                                , data
                                , DoOAEPadding
                            );
            }
        }
        public static byte[] X509CertificateSignSHA1
                                    (
                                        X509Certificate2 privateKeyPfx
                                        , byte[] data
                                    )
        {
            using (RSACryptoServiceProvider provider = privateKeyPfx.PrivateKey as RSACryptoServiceProvider)
            {
                return RSASignSHA1
                            (
                                provider
                                , data
                            );
            }
        }
        public static byte[] X509CertificateSignMD5
                                    (
                                        X509Certificate2 privateKeyPfx
                                        , byte[] data
                                    )
        {
            using (var provider = privateKeyPfx.PrivateKey as RSACryptoServiceProvider)
            {
                return RSASignMD5(provider, data);
            }
        }
        public static bool X509CertificateVerifySHA1
                                    (
                                        X509Certificate2 publicKeyCer
                                        , byte[] data
                                        , byte[] signature
                                    )
        {
            using (var provider = publicKeyCer.PublicKey.Key as RSACryptoServiceProvider)
            {
                return RSAVerifySHA1
                            (
                                provider
                                , data
                                , signature
                            );
            }
        }
        public static bool X509CertificateVerifyMD5
                                    (
                                        X509Certificate2 publicKeyCer
                                        , byte[] data
                                        , byte[] signature
                                    )
        {
            using (RSACryptoServiceProvider provider = publicKeyCer.PublicKey.Key as RSACryptoServiceProvider)
            {
                return RSAVerifyMD5
                            (
                                provider
                                , data
                                , signature
                            );
            }
        }
        public static string BytesArrayToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }
        public static byte[] HexStringToBytesArray(string text)
        {
            text = text.Replace(" ", "");
            int l = text.Length;
            byte[] buffer = new byte[l / 2];
            for (int i = 0; i < l; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(text.Substring(i, 2), 16);
            }
            return buffer;
        }
    }
}
