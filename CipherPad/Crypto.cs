using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CipherPad
{
    static class Crypto
    {
        const int PBKDF_ITERATIONS = 128000;

        public static CryptoStream OpenEncryptionStream(string password, Stream outStream)
        {
            var random = new RNGCryptoServiceProvider();
            var salt = new byte[16];
            random.GetBytes(salt);
            var derive = new Rfc2898DeriveBytes(password, salt, PBKDF_ITERATIONS);
            var verify = derive.GetBytes(4); // password verification

            outStream.Write(salt, 0, salt.Length); // write salt
            outStream.Write(verify, 0, verify.Length); // write password verification value
            var iv = salt;
            random.GetBytes(iv);
            outStream.Write(iv, 0, iv.Length); // write IV

            var aes = new AesCryptoServiceProvider();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = iv;
            aes.Key = derive.GetBytes(32);
                            
            return new CryptoStream(outStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        }

        public class IncorrectPasswordException : Exception { }

        public static CryptoStream OpenDecryptionStream(string password, Stream inStream)
        {
            var salt = new byte[16];
            inStream.Read(salt, 0, salt.Length); // read salt
            var derive = new Rfc2898DeriveBytes(password, salt, PBKDF_ITERATIONS);
            var verify = derive.GetBytes(4);
            var verifyRead = new byte[4];
            inStream.Read(verifyRead, 0, verifyRead.Length); // read password verification
            if (!Array.Equals(verify, verifyRead))
            {
                throw new IncorrectPasswordException();
            }

            var iv = salt;
            inStream.Read(iv, 0, iv.Length); // read IV
            var aes = new AesCryptoServiceProvider();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = iv;
            aes.Key = derive.GetBytes(32);

            return new CryptoStream(inStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        }
    }
}
