using Nethereum.Signer;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.IO.Pem;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FISCOBCOS.CSharpSdk.ETH
{
    public class EthUtils
    {
        /// <summary>
        /// Converts an string into a formatted string of hex digits (ex: E4 CA B2)
        /// </summary>
        /// <param name="privateKey">The string to be translated into a string of hex digits.</param>
        /// <returns>Returns a well formatted string of hex digits with spacing.</returns>
        public static string ConvertPrikToHexString(string privateKey)
        {
            TextReader ptr = new StringReader(privateKey);
            Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
            Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters ecdsaPrivateKey = (Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters)pem.ReadObject();
            var Db = ByteArrayToHexString(ecdsaPrivateKey.D.ToByteArrayUnsigned());
            return Db;
        }

        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        public static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            }

            return sb.ToString().ToLower();
        }

        /// <summary>
        /// byte to sha256byte
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte[] ConvertSHA256byte(byte[] b)
        {
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] bytes = Sha256.ComputeHash(b);

            return bytes;
        }

        /// <summary>
        /// eth Sign
        /// </summary>
        /// <param name="macdata"></param>
        /// <param name="ecdsaPrivateKey"></param>
        /// <returns>r,s,v</returns>
        public static Tuple<byte[], byte[], byte[]> Sign(byte[] macdata, ECPrivateKeyParameters ecdsaPrivateKey)
        {
            MessageSigner signer = new MessageSigner();
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(ecdsaPrivateKey);
            PemObject pemObj = new PemObject("PRIVATE KEY", privateKeyInfo.ToAsn1Object().GetEncoded());
            StringWriter strPri = new StringWriter();
            Org.BouncyCastle.Utilities.IO.Pem.PemWriter pemW = new Org.BouncyCastle.Utilities.IO.Pem.PemWriter(strPri);
            pemW.WriteObject(pemObj);
            var signData = signer.SignAndCalculateV(macdata, ConvertPrikToHexString(strPri.ToString()));
            return new Tuple<byte[], byte[], byte[]>(signData.R, signData.S, signData.V);
        }
    }
}