using FISCOBCOS.CSharpSdk;
using FISCOBCOS.CSharpSdk.Core;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.SM2;
using FISCOBCOS.CSharpSdk.Utils;
using FISCOBCOS.CSharpSdk.Utis;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FISCOBCOS.CSharpSdkTest
{
    public class GMContractTest
    {
        public string privateKey = "";
        string binCode = "";
        string abi = "";
       

        public GMContractTest()
        {
            var userKey = AccountUtils.GMGetPrivateKeyByPem(BaseConfig.DefaultPrivateKeyPemPath);
            this.privateKey = AccountUtils.GMGetPrivateKeyStrByKeyObject(userKey);
            
            //this.privateKey = "1a9275393047f5e59acfa4a31cdee48cacc7e698b0070aaab3f64f87af66606e";//国密私钥，与sdk内置prik.pem 对应私钥一样
            //this.privateKey = ecpPrivateKey;
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\" + "DefaultTest111.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\" + "DefaultTest111.bin", out binCode);
           
        }


        /// <summary>
        /// 国密，同步测试，返回区块高度
        /// </summary>
        [Fact]
        public void GetBlockNumberTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, "");
            var result = contractService.GetBlockNumber();
           var version= contractService.GetClientVersion();
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
        ///国密同步通过交易Hash获取交易回执
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GMGetReceiptByTransHashTest()
        {
            string txHash = "0x4fadfcbeba245f764f7a118c6cf1347344708095055f8b2e0a0b5ed8b5993744";
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result =  contractService.GetTranscationReceipt(txHash);

            Assert.NotNull(result.AddressOnlyWhenDeployContract);
           
        }

        /// <summary>
        ///国密同步部署合约，并得到交易回执，获得交易回执会出现有一定几率为空,建议先获得交易hash，
        ///之后再进行交易回执使用。
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GMDeployContractWithReceiptTest()
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
            string contractAddress = "0xec7c2f42110189433f994956334441fab384cb7b";//上面测试部署合约得到合约地址
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
            string contractAddress = "0x362491b04bdd84b4aafa94a98a6ae13e3cf3fb19";//上面测试部署合约得到合约地址
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
            var txHash = await contractService.DeployContractAsync(binCode,abi);
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
            string txHash = "0x30ed83cc8d70aeb52ce3e9bc665d9718a9165f94cf395525ab826c3abbea4c0f";
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result = await contractService.GetTranscationReceiptAsync(txHash);

            Assert.NotNull(result.AddressOnlyWhenDeployContract);//0x26cf8fcb783bbcc7b320a46b0d1dfff5fbb27feb
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
        /// 异步调用合约方法,本测试调用合约set方法，可以解析input和event
        /// 遇到交易hash为空，生产环境采用定时服务/队列形式，先获取交易哈希，之后再去获取对应的数据
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendTranscationWithReceiptDecodeAsyncTest()
        {

            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0x26cf8fcb783bbcc7b320a46b0d1dfff5fbb27feb";//上面测试部署合约得到合约地址
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
            string contractAddress = "0x26cf8fcb783bbcc7b320a46b0d1dfff5fbb27feb";//上面测试部署合约得到合约地址
            string functionName = "get";
            var result = await contractService.CallRequestAsync(contractAddress, abi, functionName);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, result.Output);
            Assert.NotNull(outputList);
            Assert.True(outputList[0].Result.ToString() == "123");
        }

    }
}
