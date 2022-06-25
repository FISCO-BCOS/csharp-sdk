using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.IO;

namespace FISCOBCOS.CSharpSdk.Ecdsa
{
    /// <summary>
    /// signer extends
    /// </summary>
    public static class SignerExtends
    {
        /// <summary>
        /// data signature
        /// </summary>
        /// <param name="signer">signature interface</param>
        /// <param name="key">EC private key</param>
        /// <param name="src">source data</param>
        /// <returns>signature result</returns>
        public static byte[] Sign(this ISigner signer, ECPrivateKeyParameters key, byte[] src)
        {
            signer.Init(true, key);
            signer.BlockUpdate(src, 0, src.Length);
            byte[] sign = signer.GenerateSignature();

            Asn1Sequence sequence = Asn1Sequence.GetInstance(sign);
            DerInteger r = (DerInteger)sequence[0];
            DerInteger s = (DerInteger)sequence[1];

            BigInteger[] bigs = new BigInteger[] { r.Value, s.Value };

            var N = key.Parameters.N;
            bigs = preventMalleability(bigs, N);

            byte[] bs;
            using (MemoryStream ms = new MemoryStream())
            {
                DerSequenceGenerator seq = new DerSequenceGenerator(ms);
                seq.AddObject(new DerInteger(bigs[0]));
                seq.AddObject(new DerInteger(bigs[1]));
                seq.Close();
                bs = ms.ToArray();
            }

            return bs;
        }

        /// <summary>
        /// verify signature
        /// </summary>
        /// <param name="signer">signature interface</param>
        /// <param name="key">EC public key</param>
        /// <param name="src">source data</param>
        /// <param name="signature">signature data</param>
        /// <returns></returns>
        public static bool Verify(this ISigner signer, ECPublicKeyParameters key, byte[] src, byte[] signature)
        {
            signer.Init(false, key);
            signer.BlockUpdate(src, 0, src.Length);
            return signer.VerifySignature(signature);
        }

        /// <summary>
        /// sign csr
        /// </summary>
        /// <param name="signer">signature interface</param>
        /// <param name="key">EC private key</param>
        /// <param name="src">source data</param>
        /// <returns>signature result</returns>
        public static byte[] CsrSign(this ISigner signer, AsymmetricKeyParameter key, byte[] src, BigInteger curveN)
        {
            signer.Init(true, key);
            signer.BlockUpdate(src, 0, src.Length);
            byte[] signature = signer.GenerateSignature();

            Asn1Sequence sequence = Asn1Sequence.GetInstance(signature);
            DerInteger r = (DerInteger)sequence[0];
            DerInteger s = (DerInteger)sequence[1];

            BigInteger[] sigs = new BigInteger[] { r.Value, s.Value };

            sigs = SignerExtends.preventMalleability(sigs, curveN);
            byte[] bs;
            using (MemoryStream ms = new MemoryStream())
            {
                DerSequenceGenerator seq = new DerSequenceGenerator(ms);
                seq.AddObject(new DerInteger(sigs[0]));
                seq.AddObject(new DerInteger(sigs[1]));
                seq.Close();
                bs = ms.ToArray();
            }

            return bs;
        }

        /// <summary>
        /// check if sigs is equal to crveN, if not, then replace signs
        /// </summary>
        /// <param name="sigs">signature data</param>
        /// <param name="curveN">private key parameter:N</param>
        /// <returns>signature data</returns>
        public static BigInteger[] preventMalleability(BigInteger[] sigs, BigInteger curveN)
        {
            BigInteger cmpVal = curveN.Divide(BigInteger.ValueOf(2L));
            BigInteger sval = sigs[1];

            if (sval.CompareTo(cmpVal) == 1)
            {
                sigs[1] = curveN.Subtract(sval);
            }

            return sigs;
        }
    }
}