using FISCOBCOS.CSharpSdk.Core;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FISCOBCOS.CSharpSdk.Test
{
    public class ConstructorTest
    {
        public string privateKey = "";
        string binCode = "";
        string abi = "";

        public ConstructorTest()
        {

            this.privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\erc20\\bin\\" + "Test.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\erc20\\bin\\" + "Test.bin", out binCode);

        }

        /// <summary>
        /// 部署合约，构造函数带参数合约部署
        /// </summary>
        [Fact]
        public void DeployConstructorWithParamsTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var paramsValue = new object[] { "123" };
            var txHash = contractService.DeployContract(binCode, abi, paramsValue);
            var result = contractService.GetTranscationReceipt(txHash);
            Assert.NotNull(result.AddressOnlyWhenDeployContract);
           
        }

        /// <summary>
        /// 合约调用获取Contructor设置的参数
        /// </summary>
        [Fact]
        public void GetContructorParamsTest()
        {
            //var data = "0x2dc9".HexToBigInteger(true);
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);

            string contractAddress = "0x1be74c68f3fec43b2360dd7456748121e1216960";//上面测试部署合约得到合约地址
            var funcInputsParameters = new[] { BuildParams.CreateParam("uint256", "temp") };
            var funcParamsValue = new object[] { 1 };
            string functionName = "get";//调用合约方法
          
            ReceiptResultDto receiptResultDto = contractService.CallRequest(contractAddress, abi, functionName, funcInputsParameters, funcParamsValue);
            Assert.NotEmpty(receiptResultDto.Output);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, receiptResultDto.Output);
            Assert.Equal("123",outputList[0].Result);
        }

    }
}
