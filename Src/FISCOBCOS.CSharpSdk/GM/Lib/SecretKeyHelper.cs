using System;
using System.Security.Cryptography;
using System.Text;

namespace FISCOBCOS.CSharpSdk.Lib
{
    public class SecretKeyHelper
    {
        #region Rsa Asymmetric encryption and decryption

        /// <summary>
        /// rsa encryption
        /// </summary>
        /// <param name="xmlPublicKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string xmlPublicKey, string content)
        {
            string encryptedContent = string.Empty;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(xmlPublicKey);
                byte[] encryptedData = rsa.Encrypt(Encoding.Default.GetBytes(content), false);
                encryptedContent = Convert.ToBase64String(encryptedData);
            }
            return encryptedContent;
        }

        /// <summary>
        /// rsa decryption
        /// </summary>
        /// <param name="xmlPrivateKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSADecrypt(string xmlPrivateKey, string content)

        {
            string decryptedContent = string.Empty;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())

            {
                rsa.FromXmlString(xmlPrivateKey);

                byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(content), false);

                decryptedContent = Encoding.UTF8.GetString(decryptedData);
            }

            return decryptedContent;
        }

        #endregion Rsa Asymmetric encryption and decryption

        #region AES Symmetric encryption and decryption

        /// <summary>
        /// AES encryption
        /// </summary>
        /// <param name="plainStr">plaintext string</param>
        /// <param name="key">key</param>
        /// <returns>cipher</returns>
        public static string AESEncrypt(string encryptStr, string key)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(key);
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(encryptStr);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// AES decryption
        /// </summary>
        /// <param name="plainStr">Ciphertext string</param>
        /// <param name="key">key</param>
        /// <returns>plaintext</returns>
        public static string AESDEncrypt(string encryptStr, string key)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(key);
                byte[] toEncryptArray = Convert.FromBase64String(encryptStr);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion AES Symmetric encryption and decryption
    }
}