using FISCOBCOS.CSharpSdk.Extensions;
using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore.Crypto;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace FISCOBCOS.CSharpSdk.Test
{
    public class NBitCoinTest
    {
        public string privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
        public ITestOutputHelper _testOutput;//辅助输出对应的测试结果
        public NBitCoinTest(ITestOutputHelper output)
        {
            _testOutput = output;
        }
      
        /// <summary>
        /// 助记词测试
        /// </summary>
        [Fact]
        public void WordTest()
        {
            //1、通过助记词 得到私钥
            Mnemonic mnemo = new Mnemonic("累 妙 清 董 贮 程 异 瞧 敲 发 拍 虾", Wordlist.ChineseSimplified);
            ExtKey hdRoot = mnemo.DeriveExtKey("123456");
            var privateKey = hdRoot.PrivateKey.ToBytes();//字节--》对应不同私钥体系
            var ethKey = new EthECKey(privateKey, true);
            var hdEthPrivateKey = ethKey.GetPrivateKey();//得到hdEthPrivateKey

            Mnemonic tempMnemonic = new Mnemonic("累 妙 清 董 贮 程 异 瞧 敲 发 拍 虾", Wordlist.ChineseSimplified);
            string Seed = tempMnemonic.DeriveSeed("123456").ToHex();
            //2、通过计算得到种子，获得私钥
            var masterKey = new ExtKey(Seed);
            //var masterKeyString = masterKey.PrivateKey.GetBitcoinSecret(Network.Main);
            var tempEthKey = new EthECKey(masterKey.PrivateKey.ToBytes(), true);
            var tempPrivateKey = tempEthKey.GetPrivateKey();

            Assert.Equal(hdEthPrivateKey, tempPrivateKey);
        }

        /// <summary>
        /// 通过助记词+密码+keyPath的种子 可以得到私钥
        /// 将助记词+密码记住，通过程序计算得到私钥
        /// 这里每个地址生成对应的私钥都是独立的，应为Keypath 在操作使用被替换为具体index，所以这些私钥和wallet masterKey 并不是主秘钥和子秘钥的关系
        /// </summary>
        [Fact]
        public void MoreWalletTest()
        {
            Wallet wallet = new Wallet("赛 烂 肉 什 状 系 既 株 炼 硫 辞 州", "123456");
            var pk = wallet.GetPrivateKey(0).ToHex();//ef47fca84122c17bc312d44985ebf75cd09b4beb611204b43f9f448c86cdf5e3
            var addrArray = wallet.GetAddresses(19);//获得20个地址，每一个都不一样，19 是下标
            
            var masterKey = wallet.GetMasterKey().PrivateKey.ToHex();//获得主钥
            var publicKey = wallet.GetMasterKey().Neuter();

            int index = 0;
            var keyPath = new NBitcoin.KeyPath(wallet.Path.Replace("x", index.ToString()));
            // masterKey.Derive(keyPath);

            var childKey = wallet.GetMasterKey().Derive((uint)index);
            var pubKey = wallet.GetMasterKey().Neuter();
            ExtKey recovered = childKey.GetParentExtKey(pubKey);

            ExtKey recovered0 = wallet.GetPrivateExtKey(0).Derive(0);//得到序列为0的钱包私钥，计算它的子私钥 对应序列也是0
            ExtPubKey recovered0PublicKey = wallet.GetPrivateExtKey(0).Neuter();//得到序列为0的钱包私钥 的公钥
            //var result = recovered.PrivateKey.ToHex();
            var recovered0PrivateKey = recovered0.GetParentExtKey(recovered0PublicKey);
            Assert.Equal(wallet.GetPrivateExtKey(0).PrivateKey.ToHex(),recovered0PrivateKey.PrivateKey.ToHex());


        }

        public byte[] GetPubKeyNoPrefix(byte[] pubKey)
        {
            var arr = new byte[pubKey.Length - 1];
            //remove the prefix
            Array.Copy(pubKey, 1, arr, 0, arr.Length);
            return arr;
        }

        /// <summary>
        /// hardened =false 非强密，子秘钥可以配合根公钥，计算得到根私钥，如果hardened=true，强化模式，无法使用子秘钥 找回根私钥
        /// 以太坊和比特币用了同一种椭圆曲线算法，公私钥是一样的，地址计算不一样
        /// 比特币使用Base58Check（公钥Hash（publicKey sha256和 ripemd160 hash）），以太坊是公钥的Sha3Keccack（公钥）的hash 最后的40位（hash总长度 64）
        /// </summary>
        [Fact]
        public void ExtKeyDeriveTest()
        {
            ExtKey ceoKey = new ExtKey();
            string ceoPrivateKey = ceoKey.ToString(Network.Main);//这个是比特币私钥，表现形式不一样,wif 格式
            ExtKey accountingKey = ceoKey.Derive(0, hardened: false);
            ExtKey child1Key = ceoKey.Derive(1);
            ExtKey child2Key = ceoKey.Derive(2);
            ExtPubKey ceoPubkey = ceoKey.Neuter();
            ExtKey ceoKeyRecovered0 = accountingKey.GetParentExtKey(ceoPubkey); //Crash  ceoKeyRecovered0.PrivateKey.ToHex()
                                                                                // "c54a439ed1e50f33d35bad8008d29704892b1f741498a60ad0808731ccffa90d"
            ExtKey ceoKeyRecovered1 = child1Key.GetParentExtKey(ceoPubkey); //Crash
            ExtKey ceoKeyRecovered2 = child2Key.GetParentExtKey(ceoPubkey); //Crash

            Assert.Equal(ceoKey.PrivateKey.ToHex(), ceoKeyRecovered0.PrivateKey.ToHex());
            Assert.Equal(ceoKey.PrivateKey.ToHex(), ceoKeyRecovered1.PrivateKey.ToHex());
            Assert.Equal(ceoKey.PrivateKey.ToHex(), ceoKeyRecovered2.PrivateKey.ToHex());
            //ceoKey.PrivateKey.ToHex()
            //"58e4c296dc0f83946010b7e92f04f10f7dc4cda3d423f7113b4af500a1641384"
            //ceoKey.GetPublicKey().ToHex()
            //"03e7b5843e409ea4173e9f2c8932b9b515c5bd182c8f6e3d7589829976e73d535f"
            //ceoPubkey.GetPublicKey().ToHex()
            //"03e7b5843e409ea4173e9f2c8932b9b515c5bd182c8f6e3d7589829976e73d535f"
            //ceoKeyRecovered0.PrivateKey.ToHex()
            //"58e4c296dc0f83946010b7e92f04f10f7dc4cda3d423f7113b4af500a1641384"

        }


    }
}
