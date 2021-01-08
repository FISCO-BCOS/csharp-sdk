using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore.Crypto;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FISCOBCOS.CSharpSdk.Test
{
    public class SignTest
    {

        /// <summary>
        /// RLP+私钥签名,公钥验签
        /// </summary>
        [Fact]
        public void SignRLPVerifyTest()
        {
            var privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
            var signedValue = new RLPSigner(new[] { "hello".ToBytesForRLPEncoding() });
            signedValue.Sign(new EthECKey(privateKey.HexToByteArray(), true));
            var encoded = signedValue.GetRLPEncoded();
            var hexEncoded = encoded.ToHex();
            //1-签名：环境模拟，用私钥对交易进行签名，得到hex
            //2-验签：使用签名数据+消息摘要，解签得到公钥，看看和我们留存公钥是否相等

            var signedRecovery = new RLPSigner(hexEncoded.HexToByteArray(), 1);
            var value = signedRecovery.Data[0].ToStringFromRLPDecoded();
            Assert.Equal("hello", value);
            var signEthKey = EthECKey.RecoverFromSignature(signedRecovery.Signature, signedRecovery.RawHash);
            var temp = signEthKey.Verify(signedRecovery.RawHash, signedRecovery.Signature);
            var signPublicKey = signEthKey.GetPubKeyNoPrefix().ToHex();
            //验签
            Assert.Equal("0x6a3b8f69e6860c1ad417944ae4d262930cf23ba0d1ee40ed09a4f165a2642be766901d0bd1b1d0510e0b9976ac314e961910a145073c21fdcb8cdaf8f4fbee56", "0x" + signPublicKey);

        }

        /// <summary>
        /// Msg 签名测试
        /// </summary>
        [Fact]
        public void SignMsgVerifyTest()
        {
            var signature =
              "0x0976a177078198a261faf206287b8bb93ebb233347ab09a57c8691733f5772f67f398084b30fc6379ffee2cc72d510fd0f8a7ac2ee0162b95dc5d61146b40ffa1c";
            var text = "test";
            var hasher = new Sha3Keccack();
            var hash = hasher.CalculateHash(text);
            var byteList = new List<byte>();

            var bytePrefix = "0x19".HexToByteArray();
            var textBytePrefix = Encoding.UTF8.GetBytes("Ethereum Signed Message:\n" + hash.HexToByteArray().Length);
            var bytesMessage = hash.HexToByteArray();

            byteList.AddRange(bytePrefix);
            byteList.AddRange(textBytePrefix);
            byteList.AddRange(bytesMessage);
            var hashPrefix2 = hasher.CalculateHash(byteList.ToArray()).ToHex();

            var signer = new MessageSigner();
            var privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
            var tempSignStr = signer.Sign(hashPrefix2.HexToByteArray(), privateKey);

            var account = signer.EcRecover(hashPrefix2.HexToByteArray(), tempSignStr);//私钥签名消息摘要+原始数据

            Assert.Equal("0xf827414cb1c39787d50bcebe534abe1ed2d5619f", account.EnsureHexPrefix().ToLower());

            signature = signer.Sign(hashPrefix2.HexToByteArray(),
                "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7");

        }

        /// <summary>
        /// ECDSA 签名，通过原始数据，解析得到公钥和原始公钥进行比对
        /// </summary>
        [Fact]
        public void ShouldECDSASignWithRecover()
        {

            string privateKey = "0x95b2de506d7f63be7d28ff76e1a6d2ee12ed28b2f1d929f48ecf92dc0c4ba743";//私钥
            string messageDigest = "38510ae104cf0445c6fd014973eaf668cc06a3b9e603f9e2950bdf5e968b188a";//消息摘要
            EthECKey ethECKey = new EthECKey(privateKey);
            EthECDSASignature ethECDSASignature = ethECKey.Sign(messageDigest.HexToByteArray());//r、s、有值，v没有值
            EthECDSASignature tempSign = ethECKey.SignAndCalculateV(messageDigest.HexToByteArray());

            //假设生产环节拿到签名string
            string signature = EthECDSASignature.CreateStringSignature(tempSign);
            //通过签名，得到eth的ecdsd的签名对象
            EthECDSASignature ethECDSA = EthECDSASignatureFactory.ExtractECDSASignature(signature);
            //通过消息摘要、签名对象，解析得到公钥，如果公钥和用户的公钥是一样，那么签名验证成功
            var pubKey1 = EthECKey.RecoverFromSignature(ethECDSA, messageDigest.HexToByteArray()).GetPubKey().ToHex();//消息摘要，签名，
            Assert.Equal("0480d9d565daa746fa9a9e05926c35789a6fe4f678ef8e4459dcca0fe7a1f441bc0d41d5f039b2cac4c3fca27943fa99f7125a6a677146160308fdee89dd56a637", pubKey1);//公钥验签成功

            #region 基础一些测试
            var data = "0x" + tempSign.R.ToHex().PadLeft(64, '0') +
                  tempSign.S.ToHex().PadLeft(64, '0');
            var signString = data + tempSign.V.ToHex();
            var indexValue = signString.HexToByteArray()[64];//如果 indexValue 等0或者1 需要+27
            var vData = new[] { indexValue }.ToHex();
            var pubKey = EthECKey.RecoverFromSignature(tempSign, messageDigest.HexToByteArray()).GetPubKey().ToHex();//消息摘要，签名，得到公钥, //计算得到公钥和用户公钥相等，说明签名成功;

            Assert.Equal(pubKey, pubKey1);
            Assert.Equal("cebeeb806b602b6b1661ce60d78ee8597cb7799adf4c153e0a52386b8d393fcd4e2b732bd3637f55e09aacfd1c5b1fd27be0b50c5a857ad35b76ce5a2450520b01", signature);
            #endregion
        }

        /// <summary>
        /// 添加keccak 256 和sha256 匹配以太坊，和bcos
        /// </summary>
        [Fact]
        public void HashTest()
        {
            var keccak = new Sha3Keccack();
            var result = keccak.CalculateHashFromHex(
                "0x93cdeb708b7545dc668eb9280176169d1c33cfd8ed6f04690a0bcc88a93fc4ae",
                "0x0d57c7d8b54ebf963f94cded8a57a1b109ac7465ada218575473648bf373b90d");

            string testKey = "352203199105173710林宣名&352203199105173710林宣名&352203199105173710林宣名";//计算得到hash  bytes 32
            var result1 = keccak.CalculateHash(testKey);
            string actualStr = "0x03b4e6ca8acc9b14f1674ebf6af70c5f33d5d582f3d7581d59c7fda49c9ba83e";
            var length = actualStr.HexToByteArray().Length;//32
            Assert.Equal("0x" + result1, actualStr);

            KeyStoreCrypto keyStoreCrypto = new KeyStoreCrypto();
            var sha256Key = keyStoreCrypto.CalculateSha256Hash(Encoding.UTF8.GetBytes(testKey)).ToHex();
            Assert.Equal("0x37378082a60708c400e4ad3fdaeb49248b495c764a5fa9c2bd988fe52b713b9a", "0x" + sha256Key);
            Assert.Equal("13265b3c8b785f6715b215cb1e6869312588a03afe0076beda8042c2ceb5603b", result);

        }
    }
}
