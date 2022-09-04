using FISCOBCOS.CSharpSdk;
using FISCOBCOS.CSharpSdk.Core;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Utils;
using FISCOBCOS.CSharpSdk.Utis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FISCOBCOS.CSharpSdkTest
{
    public class ContractTest
    {
        public string privateKey = "";
        string binCode = "";
        string abi = "";

        public ContractTest()
        {

            this.privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\" + "DefaultTest.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\" + "DefaultTest.bin", out binCode);

        }

        /// <summary>
        /// 同步测试，返回区块高度
        /// </summary>

        [Fact]
        public void GetBlockNumberTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result = contractService.GetBlockNumber();
            Assert.True(result > 0);
        }

        /// <summary>
        /// 同步合约部署    
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void DeployContractTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var txHash =  contractService.DeployContract(binCode, abi);
            //0x1fbfad279a915d51e4dd14a6d22cf8a437eafbd666e8a880d99d055b57f48b03
            Assert.NotNull(txHash);
           
        }

        /// <summary>
        ///同步通过交易Hash获取交易回执
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GetReceiptByTransHashTest()
        {
            string txHash = "0x1fbfad279a915d51e4dd14a6d22cf8a437eafbd666e8a880d99d055b57f48b03";
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result =  contractService.GetTranscationReceipt(txHash);

            Assert.NotNull(result.AddressOnlyWhenDeployContract);
           
        }

        /// <summary>
        ///同步部署合约，并得到交易回执，获得交易回执会出现有一定几率为空
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void DeployContractWithReceiptTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result =  contractService.DeployContractWithReceipt(binCode,abi);
            Assert.NotNull(result.AddressOnlyWhenDeployContract);//0x149d743274d91eeea8f646901fc8dd79551dccda
        }

        /// <summary>
        /// 同步调用合约方法,本测试调用合约set方法，可以解析input和event
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void SendTranscationWithReceiptDecodeTest()
        {

            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x149d743274d91eeea8f646901fc8dd79551dccda";//上面测试部署合约得到合约地址
            var inputsParameters = new[] { BuildParams.CreateParam("string", "n") };
            var paramsValue = new object[] { "123" };
            string functionName = "set";//调用合约方法
            ReceiptResultDto receiptResultDto =  contractService.SendTranscationWithReceipt(abi, contractAddress, functionName, inputsParameters, paramsValue);
            Assert.NotEmpty(receiptResultDto.Output);
            Assert.NotEmpty(receiptResultDto.Input);
            Assert.NotEmpty(receiptResultDto.Logs);
            var solidityAbi = new SolidityABI(abi);
            var inputList = solidityAbi.InputDecode(functionName, receiptResultDto.Input);
            Assert.True(inputList[0].Parameter.Name == "n" && inputList[0].Result.ToString() == "123");

            string eventName = "SetEvent";
            var eventList = solidityAbi.EventDecode(eventName, receiptResultDto.Logs);
            var eventpramas1 = eventList[0].Event.Find(x => x.Parameter.Name == "paramsStr");
            var eventpramas2 = eventList[0].Event.Find(x => x.Parameter.Name == "operationTimeStamp");
            Assert.True(eventpramas1.Result.ToString() == "123");
            Assert.NotNull(eventpramas2.Result);


        }

        /// <summary>
        /// 同步测试Call调用
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void CallRequestTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x149d743274d91eeea8f646901fc8dd79551dccda";//上面测试部署合约得到合约地址
            string functionName = "get";
            var result =  contractService.CallRequest(contractAddress, abi, functionName);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, result.Output);
            Assert.NotNull(outputList);
            Assert.True(outputList[0].Result.ToString() == "123");
        }


        /// <summary>
        /// 异步合约部署    
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeployContractAsyncTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var txHash = await contractService.DeployContractAsync(binCode);
            //0x1fbfad279a915d51e4dd14a6d22cf8a437eafbd666e8a880d99d055b57f48b03
            Assert.NotNull(txHash);
        }

        /// <summary>
        ///异步通过交易Hash获取交易回执
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetReceiptByTransHashAsyncTest()
        {
            string txHash = "0x1fbfad279a915d51e4dd14a6d22cf8a437eafbd666e8a880d99d055b57f48b03";
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result = await contractService.GetTranscationReceiptAsync(txHash);

            Assert.NotNull(result.AddressOnlyWhenDeployContract);
        }

        /// <summary>
        ///异步部署合约，并得到交易回执
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeployContractWithReceiptAsyncTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result = await contractService.DeployContractWithReceiptAsync(binCode);
            Assert.NotNull(result.AddressOnlyWhenDeployContract);//0x149d743274d91eeea8f646901fc8dd79551dccda
        }


        /// <summary>
        /// 异步调用合约方法,本测试调用合约set方法，可以解析input和event
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendTranscationWithReceiptDecodeAsyncTest()
        {

            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x149d743274d91eeea8f646901fc8dd79551dccda";//上面测试部署合约得到合约地址
            var inputsParameters = new[] { BuildParams.CreateParam("string", "n") };
            var paramsValue = new object[] { "123" };
            string functionName = "set";//调用合约方法
            ReceiptResultDto receiptResultDto = await contractService.SendTranscationWithReceiptAsync(abi, contractAddress, functionName, inputsParameters, paramsValue);

            Assert.NotEmpty(receiptResultDto.Output);
            Assert.NotEmpty(receiptResultDto.Input);
            Assert.NotEmpty(receiptResultDto.Logs);
            var solidityAbi = new SolidityABI(abi);
            var inputList = solidityAbi.InputDecode(functionName, receiptResultDto.Input);
            Assert.True(inputList[0].Parameter.Name == "n" && inputList[0].Result.ToString() == "123");

            string eventName = "SetEvent";
            var eventList = solidityAbi.EventDecode(eventName, receiptResultDto.Logs);
            var eventpramas1 = eventList[0].Event.Find(x => x.Parameter.Name == "paramsStr");
            var eventpramas2 = eventList[0].Event.Find(x => x.Parameter.Name == "operationTimeStamp");
            Assert.True(eventpramas1.Result.ToString() == "123");
            Assert.NotNull(eventpramas2.Result);


        }

        /// <summary>
        /// 异步测试Call调用
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CallRequestAsyncTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x149d743274d91eeea8f646901fc8dd79551dccda";//上面测试部署合约得到合约地址
            string functionName = "get";
            var result = await contractService.CallRequestAsync(contractAddress, abi, functionName);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, result.Output);
            Assert.NotNull(outputList);
            Assert.True(outputList[0].Result.ToString() == "123");
        }

    }
}
