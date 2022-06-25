using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Ecdsa;
using FISCOBCOS.CSharpSdk.ETH;
using FISCOBCOS.CSharpSdk.SM2;
using FISCOBCOS.CSharpSdk.Utils;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.Hex.HexConvertors;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;

namespace FISCOBCOS.CSharpSdk
{
    public class AccountUtils
    {
        #region 标准版ecc
        /// <summary>
        /// 自动生成一个用户(公钥，私钥，地址)
        /// </summary>
        /// <param name="userName">自定义用户名称</param>
        /// <returns></returns>
        public static AccountDto GeneratorAccount(string userName)
        {

            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var publicKey = "0x" + ecKey.GetPubKeyNoPrefix().ToHex();//规则是要去除前缀后的16进制，
            var privateKey = ecKey.GetPrivateKey().RemoveHexPrefix();
            var accountAddress = new Account(privateKey).Address.ToLower();//address 

            AccountDto accountDto = new AccountDto();
            accountDto.Address = accountAddress;
            accountDto.PrivateKey = privateKey;
            accountDto.PublicKey = publicKey;
            accountDto.UserName = userName;
            accountDto.Type = 0;
            return accountDto;

        }


        /// <summary>
        /// 通过私钥生成公钥
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <returns>返回公钥</returns>
        public static string GeneratorPublicKeyByPrivateKey(string privateKey)
        {
            var ecKey = new Nethereum.Signer.EthECKey(privateKey);
            var publicKey = "0x" + ecKey.GetPubKeyNoPrefix().ToHex();
            return publicKey;
        }

        /// <summary>
        ///通过私钥获得帐户地址
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string GetAddressByPrivateKey(string privateKey)
        {
            var accountAddress = new Account(privateKey).Address.ToLower();//address 
            return accountAddress;
        }


        /// <summary>
        /// 获得 EIP55 地址格式
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string ConvertToChecksumAddress(string address)
        {
            address = address.ToLower().RemoveHexPrefix();
            var addressHash = new Sha3Keccack().CalculateHash(address);
            var checksumAddress = "0x";

            for (var i = 0; i < address.Length; i++)
                if (int.Parse(addressHash[i].ToString(), NumberStyles.HexNumber) > 7)
                    checksumAddress += address[i].ToString().ToUpper();
                else
                    checksumAddress += address[i];
            return checksumAddress;
        }
        #endregion



        #region 国密版

        /// <summary>
        /// 自动生成一个用户(公钥，私钥，地址)
        /// </summary>
        /// <param name="userName">自定义用户名称</param>
        /// <returns></returns>
        public static AccountDto GMGeneratorAccount(string userName)
        {
            var sm2key = SM2Utils.GenerateKeyPair();
            ECKeyPair key = new ECKeyPair();
            key.prik = (ECPrivateKeyParameters)sm2key.Private;
            var privateKey = EthUtils.ByteArrayToHexString(key.prik.D.ToByteArrayUnsigned());
            var pubKey = (ECPublicKeyParameters)sm2key.Public;
            var pub1 = pubKey.Q.XCoord.GetEncoded();
            var pub2 = pubKey.Q.YCoord.GetEncoded();
            byte[] v = new byte[pub1.Length + pub2.Length];
            Array.Copy(pub1, 0, v, 0, pub1.Length);
            Array.Copy(pub2, 0, v, pub1.Length, pub2.Length);
            string pubKeyStr = Hex.ToHexString(v);//公钥字符串，验签要去掉04
            AccountDto accountDto = new AccountDto();
            accountDto.PrivateKey = privateKey;
            accountDto.PublicKey = pubKeyStr;
            accountDto.Address = GMGetAddress(pubKeyStr);
            accountDto.UserName = userName;
            accountDto.Type = 0;
            return accountDto;

        }

        /// <summary>
        /// 使用这个版本
        /// </summary>
        /// <param name="privateKeyPemPath"></param>
        /// <returns></returns>
        public static ECPrivateKeyParameters GMGetPrivateKeyByPem(string privateKeyPemPath)
        {
            var sm2key = SM2Utils.GenerateKeyPair();
            string pemPriKey = "";
            string pemPublicKey = "";
            FileUtils.ReadFile(privateKeyPemPath, out pemPriKey);
            TextReader ptr = new StringReader(pemPriKey);
            Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
            ECPrivateKeyParameters smPrivateKey = (ECPrivateKeyParameters)pem.ReadObject();
            return smPrivateKey;
        }

        #region
        /// <summary>
        /// 十六进制私钥字符串配套相关转换
        /// </summary>
        /// <param name="privateKeyStr"></param>
        /// <returns></returns>
        public static ECPrivateKeyParameters GMGetPrivateKey(string privateKeyStr)
        {
            byte[] privateKeyByte = privateKeyStr.Substring(privateKeyStr.StartsWith("0x") ? 2 : 0).HexToByteArray();
          
            var hexConvert= new HexBigIntegerBigEndianConvertor();
            var d = new Org.BouncyCastle.Math.BigInteger(privateKeyStr, 16);//通过十六进制转十进制获得正确d
            ECKeyGenerationParameters pa = new ECKeyGenerationParameters(GMObjectIdentifiers.sm2p256v1, new SecureRandom());
            ECPrivateKeyParameters privateKeyObj = new ECPrivateKeyParameters(d, pa.DomainParameters);
            //var d=  hexConvert.ConvertFromHex(privateKeyStr);
            //Org.BouncyCastle.Math.BigInteger d2 = new Org.BouncyCastle.Math.BigInteger(tempP.ToString());
            // Org.BouncyCastle.Math.BigInteger d = new Org.BouncyCastle.Math.BigInteger(privateKeyStr);// 这里转换的可能有问题,符号位置

            /* FISCOBCOS.CSharpSdk.SM2.SM2 sm2 = FISCOBCOS.CSharpSdk.SM2.SM2.Instance;

             string localDir = @"D:\Develop\OpenSource\BlockChain\FISCOBCOS\C# sdk\csharp-sdk\Src\FISCOBCOS.CSharpSdk.Test\KeyFile\";
             var sm2key = SM2Utils.GenerateKeyPair();
             string pemPriKey = "";
             string pemPublicKey = "";
             FileUtils.ReadFile(localDir + "tempPrivate.pem", out pemPriKey);
             TextReader ptr = new StringReader(pemPriKey);
             Org.BouncyCastle.OpenSsl.PemReader pem = new Org.BouncyCastle.OpenSsl.PemReader(ptr);
             ECPrivateKeyParameters smPrivateKey = (ECPrivateKeyParameters)pem.ReadObject();*/
            //ECPrivateKeyParameters privateKeyObj2 = new ECPrivateKeyParameters(d, pa.PublicKeyParamSet);
            /* ECPrivateKeyParameters privateKeyObj = new ECPrivateKeyParameters(d, new ECDomainParameters(sm2.ecc_curve, sm2.ecc_point_g, sm2.ecc_n));*/
            return privateKeyObj;
        }

        /// <summary>
        /// 通过私钥字符串获得公钥
        /// </summary>
        /// <param name="privateKeyStr"></param>
        /// <returns></returns>
        public static string GMGetPublicKey(string privateKeyStr)
        {

            var keyGenerator = new ECKeyPairGenerator();
            ECPublicKeyParameters pubKey = GetCorrespondingPublicKey(GMGetPrivateKey(privateKeyStr));

            var pub1 = pubKey.Q.XCoord.GetEncoded();
            var pub2 = pubKey.Q.YCoord.GetEncoded();
            byte[] v = new byte[pub1.Length + pub2.Length];
            Array.Copy(pub1, 0, v, 0, pub1.Length);
            Array.Copy(pub2, 0, v, pub1.Length, pub2.Length);
            string pubKeyStr = Hex.ToHexString(v);//公钥字符串，验签要去掉04 
            return pubKeyStr;
        }
        
        #endregion

        /// <summary>
        /// 通过私钥对象获得公钥
        /// </summary>
        /// <param name="privKey"></param>
        /// <returns></returns>
        public static ECPublicKeyParameters GetCorrespondingPublicKey(ECPrivateKeyParameters privKey)
        {
            ECDomainParameters eCDomainParameters = privKey.Parameters;
            ECPoint q = new FixedPointCombMultiplier().Multiply(eCDomainParameters.G, privKey.D);
            if (privKey.PublicKeyParamSet != null)
            {
                return new ECPublicKeyParameters(privKey.AlgorithmName, q, privKey.PublicKeyParamSet);
            }

            return new ECPublicKeyParameters(privKey.AlgorithmName, q, eCDomainParameters);
        }

        /// <summary>
        /// 通过公钥，获得地址
        /// </summary>
        /// <param name="pubKeyStr"></param>
        /// <returns></returns>
        public static string GMGetAddress(string pubKeyStr)
        {

            string addr = Hex.ToHexString(SM2Utils.SM3Hash(pubKeyStr.HexToByteArray())).Substring(24, 40);//用户地址，不带0x 
            return addr;
        }
        public static Byte[] GMGetVByte(string privateKeyStr)
        {
            var keyGenerator = new ECKeyPairGenerator();
            ECPublicKeyParameters pubKey = GetCorrespondingPublicKey(GMGetPrivateKey(privateKeyStr));

            var pub1 = pubKey.Q.XCoord.GetEncoded();
            var pub2 = pubKey.Q.YCoord.GetEncoded();
            byte[] v = new byte[pub1.Length + pub2.Length];
            Array.Copy(pub1, 0, v, 0, pub1.Length);
            Array.Copy(pub2, 0, v, pub1.Length, pub2.Length);
            return v;
        }
        #endregion
    }
}
