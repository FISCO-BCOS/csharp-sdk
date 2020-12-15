using FISOBCOS_NetSdk;
using FISOBCOS_NetSdk.SolidityCore;
using FISOBCOS_NetSdk.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FISCOBCOS_NetSdkTest
{
    public class ContractTest
    {
        /// <summary>
        /// 合约部署特色
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeployContractTest()
        {
            string abi = "";
            string binCode = "";
            string privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\" + "DefaultTest.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\" + "DefaultTest.bin", out binCode);
            var contractService = new ContractService(BaseConfig.DefaultUrl);
            var contractAddress = await contractService.DefaultDeployContract(binCode, privateKey);
            //0xf827414cb1c39787d50bcebe534abe1ed2d5619f
            Assert.NotNull(contractAddress);
        }
    }
}
