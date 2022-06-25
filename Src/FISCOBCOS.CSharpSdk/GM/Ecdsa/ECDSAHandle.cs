using FISCOBCOS.CSharpSdk.Lib;
using FISCOBCOS.CSharpSdk.Sign;
using System;

namespace FISCOBCOS.CSharpSdk.Ecdsa
{
    /// <summary>
    /// ecdsa sign help class
    /// </summary>
    public class ECDSAHandle : SignHandle
    {
        /// <summary>
        /// ecdsa key
        /// </summary>
        public ECKeyPair key { get; set; }

        public ECDSAHandle(string _prik, string _pubk)
        {
            key = new ECKeyPair();
            if (!string.IsNullOrEmpty(_prik))
            {
                key.prik = LibraryHelper.LoadPrikey(_prik);
            }
            if (!string.IsNullOrEmpty(_pubk))
            {
                key.pubk = LibraryHelper.LoadPubkey(_pubk);
            }
        }

        /// <summary>
        /// hash calculation
        /// </summary>
        /// <param name="msg">soure data</param>
        /// <returns>hash result</returns>
        public byte[] Hash(byte[] msg)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// signature
        /// </summary>
        /// <param name="digest">soure data</param>
        /// <returns>signature result</returns>
        public byte[] Sign(byte[] digest)
        {
            string curveName = "P-256";
            var nistCurve = Org.BouncyCastle.Asn1.Nist.NistNamedCurves.GetByName(curveName);

            var sign = nistCurve.Sign(key.prik, digest);
            return sign;
        }

        /// <summary>
        /// verify data signature
        /// </summary>
        /// <param name="sign">signature data</param>
        /// <param name="digest">source data</param>
        /// <returns>verify result</returns>
        public bool Verify(byte[] sign, byte[] digest)
        {
            string curveName = "P-256";
            var nistCurve = Org.BouncyCastle.Asn1.Nist.NistNamedCurves.GetByName(curveName);

            return nistCurve.Verify(key.pubk, digest, sign);
        }
    }
}