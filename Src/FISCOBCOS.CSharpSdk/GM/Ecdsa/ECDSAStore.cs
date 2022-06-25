using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;
using System;
using System.IO;

namespace FISCOBCOS.CSharpSdk.Ecdsa
{
    /// <summary>
    /// ECDSA store
    /// </summary>
    public class ECDSAStore
    {
        /// <summary>
        /// save private key
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="pkUrl">key file</param>
        public static void SavePriKey(AsymmetricKeyParameter privateKey, string pkUrl)
        {
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
            PemObject pemObj = new PemObject("PRIVATE KEY", privateKeyInfo.ToAsn1Object().GetEncoded());
            StringWriter strPri = new StringWriter();
            Org.BouncyCastle.Utilities.IO.Pem.PemWriter pemW = new Org.BouncyCastle.Utilities.IO.Pem.PemWriter(strPri);
            pemW.WriteObject(pemObj);
            byte[] priInfoByte = System.Text.Encoding.UTF8.GetBytes(strPri.ToString());
            FileStream fs = new FileStream(pkUrl, FileMode.Create, FileAccess.Write);
            fs.Write(priInfoByte, 0, priInfoByte.Length);
            fs.Close();
        }

        /// <summary>
        /// save public key
        /// </summary>
        /// <param name="publicKey">public key</param>
        /// <param name="pkUrl">key file</param>
        /// <returns></returns>
        public static bool SavePubKey(AsymmetricKeyParameter publicKey, string pkUrl)
        {
            try
            {
                //save public key
                SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);

                Asn1Object aobject = publicKeyInfo.ToAsn1Object();
                byte[] pubInfoByte = aobject.GetEncoded();
                FileStream fs = new FileStream(pkUrl, FileMode.Create, FileAccess.Write);
                fs.Write(pubInfoByte, 0, pubInfoByte.Length);
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// save public key
        /// </summary>
        /// <param name="publicKey">public key</param>
        /// <param name="pkUrl">key file</param>
        /// <returns></returns>
        public static bool SavePubKey(ECPublicKeyParameters publicKey, string pkUrl)
        {
            try
            {
                //save public key
                SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
                PemObject pemObj = new PemObject("PUBLIC KEY", publicKeyInfo.ToAsn1Object().GetEncoded());
                StringWriter strPri = new StringWriter();
                Org.BouncyCastle.Utilities.IO.Pem.PemWriter pemW = new Org.BouncyCastle.Utilities.IO.Pem.PemWriter(strPri);
                pemW.WriteObject(pemObj);
                byte[] priInfoByte = System.Text.Encoding.UTF8.GetBytes(strPri.ToString());
                FileStream fs = new FileStream(pkUrl, FileMode.Create, FileAccess.Write);
                fs.Write(priInfoByte, 0, priInfoByte.Length);
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}