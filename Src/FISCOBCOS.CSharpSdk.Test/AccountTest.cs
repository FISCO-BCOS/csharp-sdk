using FISCOBCOS.CSharpSdk;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Ecdsa;
using FISCOBCOS.CSharpSdk.ETH;
using FISCOBCOS.CSharpSdk.SM2;
using FISCOBCOS.CSharpSdk.Utils;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace FISCOBCOS.CSharpSdkTest
{
    public class AccountTest
    {
        public ITestOutputHelper _testOutput;//辅助输出对应的测试结果
        public AccountTest(ITestOutputHelper output)
        {
            _testOutput = output;
        }
        /// <summary>
        /// 生成一对公私钥，生成的json可以copy 到txt文件，直接导入webase front 等组件中
        /// </summary>
        [Fact]
        public void GeneratorAccountTest()
        {
            var account = AccountUtils.GeneratorAccount("adminUser" + new Random().Next(100000, 1000000).ToString());
            var accountString = account.ToJson();
            // Debug.WriteLine(accountString);
            _testOutput.WriteLine(accountString);
            Assert.True(accountString.ToObject<AccountDto>().PublicKey.Length > 0);
        }

        /// <summary>
        /// 通过私钥生成公钥
        /// </summary>
        [Fact]
        public void GeneratorPublicKeyByPrivateKeyTest()
        {
            var publicKey = AccountUtils.GeneratorPublicKeyByPrivateKey("25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff");
            Assert.True(publicKey == "0x6a3b8f69e6860c1ad417944ae4d262930cf23ba0d1ee40ed09a4f165a2642be766901d0bd1b1d0510e0b9976ac314e961910a145073c21fdcb8cdaf8f4fbee56");
            var initaddr = new Sha3Keccack().CalculateHash(publicKey.Substring(publicKey.StartsWith("0x") ? 2 : 0).HexToByteArray());
            var addr = new byte[initaddr.Length - 12];
            Array.Copy(initaddr, 12, addr, 0, initaddr.Length - 12);
            var address = AccountUtils.ConvertToChecksumAddress(addr.ToHex());
            //initaddr.ToHex() 长度64，截取后面40位得到就是地址
            Assert.Equal(initaddr.ToHex().Substring(24, 40), address.RemoveHexPrefix().ToLower());
        }


        /// <summary>
        /// 通过私钥获取账户地址
        /// </summary>
        [Fact]
        public void GetAddressByPrivateKeyTest()
        {
            var address = AccountUtils.GetAddressByPrivateKey("25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff");
            Assert.Equal("0xf827414cb1c39787d50bcebe534abe1ed2d5619f", address);

        }


        /// <summary>
        /// 生成一对国密公私钥
        /// </summary>
        [Fact]
        public void GMGeneratorAccountTest()
        {
            string test = System.Environment.CurrentDirectory;
            string localDir = @"D:\Develop\OpenSource\BlockChain\FISCOBCOS\C# sdk\csharp-sdk\Src\FISCOBCOS.CSharpSdk.Test\KeyFile\";
            var sm2key = SM2Utils.GenerateKeyPair();
            ECDSAStore.SavePriKey((ECPrivateKeyParameters)sm2key.Private, localDir+"prik.pem");
            ECDSAStore.SavePubKey((ECPublicKeyParameters)sm2key.Public, localDir + "pubk.pem");

            #region 解析对应公私钥的字符串
            ECKeyPair key = new ECKeyPair();
            key.prik = (ECPrivateKeyParameters)sm2key.Private;
            var Db = EthUtils.ByteArrayToHexString(key.prik.D.ToByteArrayUnsigned());
            key.pubk = (ECPublicKeyParameters)sm2key.Public;

            var pub1 = key.pubk.Q.XCoord.GetEncoded();
            var pub2 = key.pubk.Q.YCoord.GetEncoded();
            byte[] v = new byte[pub1.Length + pub2.Length];
            Array.Copy(pub1, 0, v, 0, pub1.Length);
            Array.Copy(pub2, 0, v, pub1.Length, pub2.Length);
            Console.WriteLine(string.Format("sign.V:{0}", Hex.ToHexString(v))); 
            #endregion


        }

       

        /// <summary>
        /// 导入Pem,并进行解析
        /// </summary>
        [Fact]
        public void GMImportAccountTest()
        {
            string localDir = @"D:\Develop\OpenSource\BlockChain\FISCOBCOS\C# sdk\csharp-sdk\Src\FISCOBCOS.CSharpSdk.Test\KeyFile\";
            var sm2key = SM2Utils.GenerateKeyPair();
            string pemPriKey = "";
            string pemPublicKey = "";
            FileUtils.ReadFile(localDir + "prik.pem", out pemPriKey);
            TextReader ptr = new StringReader(pemPriKey);
            Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
            ECPrivateKeyParameters smPrivateKey= (ECPrivateKeyParameters)pem.ReadObject();
            var privateKeyStr = EthUtils.ByteArrayToHexString(smPrivateKey.D.ToByteArrayUnsigned());//私钥字符串

            FileUtils.ReadFile(localDir + "pubk.pem", out pemPublicKey);
            TextReader pubtr = new StringReader(pemPublicKey);
            Org.BouncyCastle.OpenSsl.PemReader pubPem = new Org.BouncyCastle.OpenSsl.PemReader(pubtr);
            ECPublicKeyParameters smPublicKey = (ECPublicKeyParameters)pubPem.ReadObject();
           
            #region 将私钥字符串，逆转为对象
            string tempPrik = "98b8c3cd41f8ab0c7ec0cc19f1dd0bd087b8372280e02cfc51dd7e6d82736015";
            
            byte[] privateKeyByte = tempPrik.Substring(privateKeyStr.StartsWith("0x") ? 2 : 0).HexToByteArray();
            var d = new Org.BouncyCastle.Math.BigInteger(privateKeyStr, 16);//通过十六进制转十进制获得正确d

            SM2 sm2 = SM2.Instance;
           
            ECPrivateKeyParameters privateKeyObj = new ECPrivateKeyParameters(d, new ECDomainParameters(sm2.ecc_curve, sm2.ecc_point_g, sm2.ecc_n));
            
            #endregion
            #region 生成公钥字符串
            var keyGenerator = new ECKeyPairGenerator();
            ECPublicKeyParameters pubKey = GetCorrespondingPublicKey(privateKeyObj);

            var pub1 = pubKey.Q.XCoord.GetEncoded();
            var pub2 = pubKey.Q.YCoord.GetEncoded();
            byte[] pubv = new byte[pub1.Length + pub2.Length];
            Array.Copy(pub1, 0, pubv, 0, pub1.Length);
            Array.Copy(pub2, 0, pubv, pub1.Length, pub2.Length);
            string pubKeyStr = Hex.ToHexString(pubv);//公钥字符串，验签要去掉04 
            #endregion
            #region 生成用户地址
            string addr = Hex.ToHexString(SM2Utils.SM3Hash(pubv)).Substring(24, 40);//用户地址，不带0x 
            #endregion
        }

        /// <summary>
        /// 私钥字符串 变成私钥对象
        /// </summary>
        [Fact]
        public void GMPublicKeyTest() {

            string localDir = @"D:\Develop\OpenSource\BlockChain\FISCOBCOS\C# sdk\csharp-sdk\Src\FISCOBCOS.CSharpSdk.Test\KeyFile\";
            var sm2key = SM2Utils.GenerateKeyPair();
            string pemPriKey = "";
            string pemPublicKey = "";
            FileUtils.ReadFile(localDir + "tempPrivate.pem", out pemPriKey);
            TextReader ptr = new StringReader(pemPriKey);
            Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
            ECPrivateKeyParameters smPrivateKey = (ECPrivateKeyParameters)pem.ReadObject();
            var privateKeyStr = EthUtils.ByteArrayToHexString(smPrivateKey.D.ToByteArrayUnsigned());//私钥字符串

        }

        /// <summary>
        /// 国密生成一对公私钥，生成的json可以copy 到txt文件，直接导入webase front 等组件中
        /// </summary>
        [Fact]
        public void GMGeneratorAccountJsonTest() 
        {
            var account = AccountUtils.GMGeneratorAccount("adminUser" + new Random().Next(100000, 1000000).ToString());
            var accountString = account.ToJson();
            // Debug.WriteLine(accountString);
            _testOutput.WriteLine(accountString);
            Assert.True(accountString.ToObject<AccountDto>().PublicKey.Length > 0);
        }

        /// <summary>
        /// 加密解密 
        /// </summary>
        [Fact]
        public void CryptTest()
        {
            string privateKey = "7f837f88b9bf54139e6ce97dc739f12e0c547b276c65bfb95bafa2b3e4ecef32";
            string publicKey = "046f5af7d582070f42d68e9834fd560435a41d168b20bd42e1822ba6b86c1283560cc53cfd0b30b85a5587a913f126d69feba2826649b5aa86a677d03fd48ac7a7";
            byte[] testData = privateKey.HexToByteArray();
            string encryptData= SM2Utils.Encrypt(publicKey.HexToByteArray(), testData);
           string decryptStr= SM2Utils.Decrypt(privateKey.HexToByteArray(), encryptData.HexToByteArray()).ToHex();
        }


        internal static ECPublicKeyParameters GetCorrespondingPublicKey(ECPrivateKeyParameters privKey)
        {
            ECDomainParameters eCDomainParameters = privKey.Parameters;
            ECPoint q = new FixedPointCombMultiplier().Multiply(eCDomainParameters.G, privKey.D);
            if (privKey.PublicKeyParamSet != null)
            {
                return new ECPublicKeyParameters(privKey.AlgorithmName, q, privKey.PublicKeyParamSet);
            }

            return new ECPublicKeyParameters(privKey.AlgorithmName, q, eCDomainParameters);
        }
    }
}
