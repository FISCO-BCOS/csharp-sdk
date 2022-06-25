using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace FISCOBCOS.CSharpSdk.Ecdsa
{
    public class ECDSAUtils
    {
        /// <summary>
        /// generate secP256r1 key
        /// </summary>
        public static AsymmetricCipherKeyPair GenerateSecP256r1KeyPair()
        {
            var keyGenerator = new ECKeyPairGenerator();
            ECKeyGenerationParameters pa = new ECKeyGenerationParameters(SecObjectIdentifiers.SecP256r1, new SecureRandom());
            keyGenerator.Init(pa);
            return keyGenerator.GenerateKeyPair();
        }

        /// <summary>
        /// generate secP256k1 key
        /// </summary>
        public static AsymmetricCipherKeyPair GenerateSecP256k1KeyPair()
        {
            var keyGenerator = new ECKeyPairGenerator();
            ECKeyGenerationParameters pa = new ECKeyGenerationParameters(SecObjectIdentifiers.SecP256k1, new SecureRandom());
            keyGenerator.Init(pa);
            return keyGenerator.GenerateKeyPair();
        }
    }
}