using FISCOBCOS.CSharpSdk;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using System;
using System.Diagnostics;
using System.Globalization;
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


    }
}
