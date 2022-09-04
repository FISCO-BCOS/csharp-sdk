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
    public class TraceEvidenceContractTest
    {
        public string privateKey = "";
        string binCode = "";
        string abi = "";

        public TraceEvidenceContractTest()
        {

            var userKey = AccountUtils.GMGetPrivateKeyByPem(BaseConfig.DefaultPrivateKeyPemPath);
            this.privateKey = AccountUtils.GMGetPrivateKeyStrByKeyObject(userKey);
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TraceEvidence\\" + "EvidenceConV1.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TraceEvidence\\" + "EvidenceConV1.bin", out binCode);

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
            string txHash = "0x2a0fa31a5628179b374b4c652664ca153c04c7e507caa1c8709903dfc26f1841";
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
        /// 同步调用合约方法
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void SendTranscationWithReceiptDecodeTest()
        {

            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0xe62159402a58a7ccc4c76ddfa10013776ee467c0";//上面测试部署合约得到合约地址
            var inputsParameters = new[] { BuildParams.CreateParam("string", "serviceId"), 
                BuildParams.CreateParam("string", "typeName"),
                BuildParams.CreateParam("string", "dataValue") };
            string tempServiceId = "abcd" + new Random().Next(1, 99999999);
            var paramsValue = new object[] { tempServiceId, "123", "123" };
            string functionName = "registerServiceData";//调用合约方法
            ReceiptResultDto receiptResultDto =  contractService.SendTranscationWithReceipt(abi, contractAddress, functionName, inputsParameters, paramsValue);
           /* Assert.NotEmpty(receiptResultDto.Output);
            Assert.NotEmpty(receiptResultDto.Input);
            Assert.NotEmpty(receiptResultDto.Logs);*/
            var solidityAbi = new SolidityABI(abi);
            var inputList = solidityAbi.InputDecode(functionName, receiptResultDto.Input);
            var outputList = solidityAbi.OutputDecode(functionName, receiptResultDto.Output);
            Assert.True(inputList[0].Parameter.Name == "serviceId" && inputList[0].Result.ToString() == tempServiceId);

            string eventName = "RegisterEvent";
            var eventList = solidityAbi.EventDecode(eventName, receiptResultDto.Logs);
            var eventpramas1 = eventList[0].Event.Find(x => x.Parameter.Name == "serviceId");
            var eventpramas2 = eventList[0].Event.Find(x => x.Parameter.Name == "typeName");
            var eventpramas3 = eventList[0].Event.Find(x => x.Parameter.Name == "dataValue");
            Assert.True(eventpramas1.Result.ToString() == tempServiceId);
            Assert.NotNull(eventpramas2.Result);


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
        ///异步部署合约，并得到交易回执
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeployContractWithReceiptAsyncTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result = await contractService.DeployContractWithReceiptAsync(binCode,abi);
            Assert.NotNull(result.AddressOnlyWhenDeployContract);//0x149d743274d91eeea8f646901fc8dd79551dccda
        }

        /// <summary>
        /// 异步调用合约方法,本测试调用合约registerServiceData方法，可以解析input和event
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendTranscationWithTxHashAsyncTest()
        {

            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x7c60ca81e8de240b20154c4fd1ff9dfaf417854f";//上面测试部署合约得到合约地址
            var inputsParameters = new[] { BuildParams.CreateParam("string", "serviceId"),
                BuildParams.CreateParam("string", "typeName"),
                BuildParams.CreateParam("string", "dataValue") };
            var paramsValue = new object[] { "asasa", "456", "东方闪电" };
            string functionName = "registerServiceData";//调用合约方法

            string txHash= await contractService.SendTranscationWithTransHashAsync(abi, contractAddress, functionName, inputsParameters, paramsValue);
            Assert.NotNull(txHash);


        }

        /// <summary>
        ///异步通过交易Hash获取交易回执
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetReceiptByTransHashAsyncTest()
        {
            string txHash = "0xa17e72e5b509ddb42d1b180b37c3e69487df187b988f2e683e1b4ff48083182d";
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result = await contractService.GetTranscationReceiptAsync(txHash);

            Assert.NotNull(result.To);
        }

        /// <summary>
        /// 异步调用合约方法
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendTranscationWithReceiptDecodeAsyncTest()
        {

            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x8e4086a9533086171e100a1604b1419bc9682ecf";//上面测试部署合约得到合约地址
            var inputsParameters = new[] { BuildParams.CreateParam("string", "serviceId"),
                BuildParams.CreateParam("string", "typeName"),
                BuildParams.CreateParam("string", "dataValue") };
            var paramsValue = new object[] { "sdsd121", "456", "东方闪电" };
            string functionName = "registerServiceData";//调用合约方法
        
            ReceiptResultDto receiptResultDto = await contractService.SendTranscationWithReceiptAsync(abi, contractAddress, functionName, inputsParameters, paramsValue);

           /* Assert.NotEmpty(receiptResultDto.Output);
            Assert.NotEmpty(receiptResultDto.Input);
            Assert.NotEmpty(receiptResultDto.Logs);*/
            var solidityAbi = new SolidityABI(abi);
            var inputList = solidityAbi.InputDecode(functionName, receiptResultDto.Input);
            var outputList = solidityAbi.OutputDecode(functionName, receiptResultDto.Output);
            Assert.True(inputList[0].Parameter.Name == "serviceId" && inputList[0].Result.ToString() == "sdsd121");



        }

        /// <summary>
        /// 异步测试Call调用
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CallRequestAsyncTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x7c60ca81e8de240b20154c4fd1ff9dfaf417854f";//上面测试部署合约得到合约地址
            string functionName = "getServiceList";

            var inputsParameters = new[] { BuildParams.CreateParam("string", "serviceId") };
            var paramsValue = new object[] { "sdsd121" };
            var result = await contractService.CallRequestAsync(contractAddress, abi, functionName, inputsParameters,paramsValue);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, result.Output);
            Assert.NotNull(outputList);
            Assert.True(outputList[0].Result.ToString() == "123");
        }

    }
}
