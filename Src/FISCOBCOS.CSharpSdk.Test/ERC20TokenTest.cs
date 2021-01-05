using FISCOBCOS.CSharpSdk.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FISCOBCOS.CSharpSdk.Test
{
    public class ERC20TokenTest
    {
        public string privateKey = "";
        string binCode = "";
        string abi = "";

        public ERC20TokenTest()
        {

            this.privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\erc20\\bin\\" + "Test.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\erc20\\bin\\" + "Test.bin", out binCode);

        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void DeployERC20TokenTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var txHash = contractService.DeployContract(binCode);
            //0x1fbfad279a915d51e4dd14a6d22cf8a437eafbd666e8a880d99d055b57f48b03
            Assert.NotNull(txHash);
        }

    }
}
