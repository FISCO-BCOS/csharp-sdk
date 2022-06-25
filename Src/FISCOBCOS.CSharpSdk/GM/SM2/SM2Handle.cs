//using FISCOBCOS.CSharpSdk.Lib;
//using FISCOBCOS.CSharpSdk.Sign;
//using Org.BouncyCastle.Security;

//namespace FISCOBCOS.CSharpSdk.SM2
//{
//    public class SM2Handle : SignHandle
//    {
//        public ECKeyPair key;

//        public SM2Handle(string _prik, string _pubk)
//        {
//            key = new ECKeyPair();
//            if (!string.IsNullOrEmpty(_prik))
//            {
//                key.prik = LibraryHelper.LoadPrikey(_prik);
//            }
//            if (!string.IsNullOrEmpty(_pubk))
//            {
//                key.pubk = LibraryHelper.LoadPubkey(_pubk);
//            }            
//        }

//        public byte[] Hash(byte[] msg)
//        {
//            return DigestUtilities.CalculateDigest("SM3", msg);
//        }

//        public byte[] Sign(byte[] digest)
//        {
//            return SM2Utils.Sign(Hash(digest), key.prik);
//        }

//        public bool Verify(byte[] sign, byte[] digest)
//        {
//            return SM2Utils.VerifyData(Hash(digest), sign, key.pubk);
//        }
//    }
//}