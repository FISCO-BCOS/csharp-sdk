using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace FISCOBCOS.CSharpSdk.Ecdsa
{
    /// <summary>
    /// X9ECParametersExtends class
    /// </summary>
    public static class X9ECParametersExtends
    {
        /// <summary>
        /// get signer instance
        /// </summary>
        /// <param name="ec">X9E parameter obj</param>
        /// <returns>signer instance</returns>
        public static ISigner GetSigner(this X9ECParameters ec)
        {
            int bits = (ec.Curve.FieldSize == 521) ? 512 : ec.Curve.FieldSize;
            return SignerUtilities.GetSigner(string.Format("SHA-{0}withECDSA", bits));
        }

        /// <summary>
        /// sign data
        /// </summary>
        /// <param name="ec">X9E parameter obj</param>
        /// <param name="key">EC private key</param>
        /// <param name="src">source data</param>
        /// <returns>signature result</returns>
        public static byte[] Sign(this X9ECParameters ec, ECPrivateKeyParameters key, byte[] src)
        {
            return ec.GetSigner().Sign(key, src);
        }

        /// <summary>
        /// verify data signature
        /// </summary>
        /// <param name="ec">X9E parameter obj</param>
        /// <param name="key">EC public key</param>
        /// <param name="src">source data</param>
        /// <param name="signature">signature data</param>
        /// <returns></returns>
        public static bool Verify(this X9ECParameters ec, ECPublicKeyParameters key, byte[] src, byte[] signature)
        {
            return ec.GetSigner().Verify(key, src, signature);
        }

        /// <summary>
        /// sign Csr
        /// </summary>
        /// <param name="ec">X9E parameter obj</param>
        /// <param name="key">EC private key</param>
        /// <param name="src">source data</param>
        /// <param name="curveN">private key parameter:N</param>
        /// <returns>signature result</returns>
        public static byte[] CsrSign(this X9ECParameters ec, AsymmetricKeyParameter key, byte[] src, BigInteger curveN)
        {
            return ec.GetSigner().CsrSign(key, src, curveN);
        }
    }
}