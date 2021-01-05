using Nethereum.Hex.HexConvertors.Extensions;
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
    }
}
