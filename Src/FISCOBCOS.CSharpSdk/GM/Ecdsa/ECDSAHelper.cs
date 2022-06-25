using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using System;
using System.IO;
using System.Text;

namespace FISCOBCOS.CSharpSdk.Ecdsa
{
    public class ECDSAHelper
    {
        /// <summary>
        /// sign data
        /// </summary>
        /// <param name="macdata">source data</param>
        /// <param name="pkInfo">private key</param>
        /// <returns>signature result</returns>
        public static byte[] SignData(string macdata, string pkInfo)
        {
            byte[] data = Encoding.UTF8.GetBytes(macdata);
            string privateKey;
            if (!pkInfo.Contains("PRIVATE KEY"))
            {
                privateKey = ReadPK(pkInfo);
            }
            else
            {
                privateKey = pkInfo;
            }
            TextReader ptr = new StringReader(privateKey);
            Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
            Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters ecdsaPrivateKey = (Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters)pem.ReadObject();
            string curveName = "P-256";
            var nistCurve = Org.BouncyCastle.Asn1.Nist.NistNamedCurves.GetByName(curveName);

            var sign = nistCurve.Sign(ecdsaPrivateKey, data);

            return sign;
        }

        /// <summary>
        /// sign data
        /// </summary>
        /// <param name="macdata">soure data</param>
        /// <param name="pkInfo">private key</param>
        /// <returns>signature result</returns>
        public static byte[] SignData(Google.Protobuf.ByteString macdata, string pkInfo)
        {
            byte[] data = macdata.ToByteArray();
            string privateKey;
            if (!pkInfo.Contains("PRIVATE KEY"))
            {
                privateKey = ReadPK(pkInfo);
            }
            else
            {
                privateKey = pkInfo;
            }
            TextReader ptr = new StringReader(privateKey);
            Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
            Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters ecdsaPrivateKey = (Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters)pem.ReadObject();
            string curveName = "P-256";
            var nistCurve = Org.BouncyCastle.Asn1.Nist.NistNamedCurves.GetByName(curveName);

            var sign = nistCurve.Sign(ecdsaPrivateKey, data);

            return sign;
        }

        /// <summary>
        /// verify data signature with public key
        /// </summary>
        /// <param name="macdata">source data</param>
        /// <param name="mac">signature string</param>
        /// <param name="pkInfo">public key</param>
        /// <returns>verify result</returns>
        public static bool VerifyData(string macdata, string mac, string pkInfo)
        {
            byte[] data = Encoding.UTF8.GetBytes(macdata);
            byte[] signature = Convert.FromBase64String(mac);
            string pub;
            if (!pkInfo.Contains("CERTIFICATE") && !pkInfo.Contains("PUBLIC KEY"))
            {
                pub = ReadPK(pkInfo);
            }
            else
            {
                pub = pkInfo;
            }

            Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters ecdsaPublicKey = null;
            if (pub.Contains("CERTIFICATE"))
            {
                Org.BouncyCastle.X509.X509Certificate cert = new Org.BouncyCastle.X509.X509CertificateParser().ReadCertificate(Encoding.UTF8.GetBytes(pub));

                ecdsaPublicKey = (Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters)cert.GetPublicKey();
            }
            else if (pub.Contains("PUBLIC KEY"))
            {
                TextReader ptr = new StringReader(pub);
                Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
                ecdsaPublicKey = (Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters)pem.ReadObject();
            }

            string curveName = "P-256";
            var nistCurve = Org.BouncyCastle.Asn1.Nist.NistNamedCurves.GetByName(curveName);

            return nistCurve.Verify(ecdsaPublicKey, data, signature);
        }

        /// <summary>
        /// signature csr
        /// </summary>
        /// <param name="macdata">source data</param>
        /// <param name="key">private key</param>
        /// <param name="curveN">private key parameter:N</param>
        /// <returns>signature result</returns>
        public static byte[] CsrSignData(byte[] macdata, AsymmetricKeyParameter key, BigInteger curveN)
        {
            string curveName = "P-256";
            var nistCurve = Org.BouncyCastle.Asn1.Nist.NistNamedCurves.GetByName(curveName);

            var sign = nistCurve.CsrSign(key, macdata, curveN);

            return sign;
        }

        /// <summary>
        /// read key file
        /// </summary>
        /// <param name="keyUrl">file path</param>
        /// <returns>content of file</returns>
        public static string ReadPK(string keyUrl)
        {
            string pk = string.Empty;
            FileStream fileStream = new FileStream(keyUrl, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                pk = reader.ReadToEnd();
            }
            return pk;
        }
    }
}