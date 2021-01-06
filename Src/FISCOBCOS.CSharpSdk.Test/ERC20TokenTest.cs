using FISCOBCOS.CSharpSdk.Core;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Utils;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\erc20\\bin\\" + "HashToken.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\erc20\\bin\\" + "HashToken.bin", out binCode);

        }

        /// <summary>
        /// 部署复合ERC20 标准的数字资产合约
        /// </summary>
        [Fact]
        public void DeployERC20TokenTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var paramsValue = new object[] { "xx数字资产", "HTC" };
            var result = contractService.DeployContractWithReceipt(binCode, abi, paramsValue);
            Assert.NotNull(result.ContractAddress);
        }

        /// <summary>
        ///异步 部署复合ERC20 标准的数字资产合约
        /// </summary>
        [Fact]
        public async Task DeployERC20TokenAsyncTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var paramsValue = new object[] { "xx数字资产", "HTC" };
            var result = await contractService.DeployContractWithReceiptAsync(binCode, abi, paramsValue);
            Assert.NotNull(result.ContractAddress);//0xa8059ddb27e30e795c01e9b226a977ec108ac05c
        }


        /// <summary>
        ///获取基础erc20 基础信息
        /// </summary>
        [Fact]
        public async Task GetTokenInfoAsyncTest()
        {
            //var data = "0x2dc9".HexToBigInteger(true);
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);

            string contractAddress = "0xa8059ddb27e30e795c01e9b226a977ec108ac05c";//上面测试部署合约得到合约地址         
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("name", "");//资产名称
            dic.Add("symbol", "");//资产代号
            dic.Add("decimals", "");// 小数位
            dic.Add("totalSupply", "");//发行资产
            Dictionary<string, string> resultDic = new Dictionary<string, string>();
            foreach (var i in dic)
            {
                ReceiptResultDto receiptResultDto = await contractService.CallRequestAsync(contractAddress, abi, i.Key);
                Assert.NotEmpty(receiptResultDto.Output);
                var solidityAbi = new SolidityABI(abi);
                var outputList = solidityAbi.OutputDecode(i.Key, receiptResultDto.Output);
                var temp = outputList[0].Result.ToString();
                if (i.Key == "totalSupply")
                {
                    int decimalsCount = int.Parse(resultDic["decimals"]);
                    temp = temp.Remove(temp.Length - (1 + decimalsCount), decimalsCount);
                }
                resultDic.TryAdd(i.Key, temp);
            }

            Tuple<string, string, string> temp1 = new Tuple<string, string, string>("", "", "");
            Assert.Equal("xx数字资产", resultDic["name"]);
            Assert.Equal("HTC", resultDic["symbol"]);
            Assert.Equal("0", resultDic["decimals"]);
            Assert.Equal("1000000", resultDic["totalSupply"]);
        }

        /// <summary>
        ///转账
        ///
        /// 测试账号{"address":"0x4d1a493a93effee00171b0b9c96fc3d62066febf","publicKey":"0x379e93ddc05f1884bb544efbaab38eab7636f830a92e268d91232c6ed4bbebc5576196a68ea53e70636cc6a2e987d1fc680d5d7ea4abe41b507736740f9cd29c","privateKey":"3d3b37d27af1fe316f44913363e6edfa9fabf95f70f6e0754fdeedcd619ab651","userName":"adminUser171982","type":0}
        /// 
        /// </summary>
        [Fact]
        public async Task TransferAysncTest()
        {
            #region 1-转账交易 给测试用户 0x4d1a493a93effee00171b0b9c96fc3d62066febf 转100 HTC
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var tempUserAddress = "0x4d1a493a93effee00171b0b9c96fc3d62066febf";//接受转账的用户地址
            string contractAddress = "0xa8059ddb27e30e795c01e9b226a977ec108ac05c";//上面测试部署合约得到合约地址
            var inputsParameters = new[] { BuildParams.CreateParam("address", "to"), BuildParams.CreateParam("uint256", "value") };
            var paramsValue = new object[] { tempUserAddress, 2000 };//给指定账号转2000
            string functionName = "transfer";//调用合约方法

            ///1、账号转账
            ReceiptResultDto receiptResultDto = await contractService.SendTranscationWithReceiptAsync(abi, contractAddress, functionName, inputsParameters, paramsValue);
            Assert.NotEmpty(receiptResultDto.Output);
            Assert.NotEmpty(receiptResultDto.Input);
            Assert.NotEmpty(receiptResultDto.Logs);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, receiptResultDto.Output);
            Assert.True(outputList[0].Parameter.Name == "" && Convert.ToBoolean(outputList[0].Result) == true);
            string eventName = "Transfer";
            var eventList = solidityAbi.EventDecode(eventName, receiptResultDto.Logs);
            var eventpramas1 = eventList[0].Event.Find(x => x.Parameter.Name == "from");
            var eventpramas2 = eventList[0].Event.Find(x => x.Parameter.Name == "to");
            var eventpramas3 = eventList[0].Event.Find(x => x.Parameter.Name == "value");
            Assert.True(eventpramas1.Result.ToString() == "0xf827414cb1c39787d50bcebe534abe1ed2d5619f");//发起方的账户地址
            Assert.True(eventpramas2.Result.ToString() == tempUserAddress);
            Assert.True(long.Parse(eventpramas3.Result.ToString()) == 2000);
            #endregion
        }

        /// <summary>
        /// 查询账户地址的余额
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAccountBalanceAysncTest()
        {
            string contractAddress = "0xa8059ddb27e30e795c01e9b226a977ec108ac05c";//上面测试部署合约得到合约地址

            var tempUserAddress = "0x4d1a493a93effee00171b0b9c96fc3d62066febf";//接受转账的用户地址
                                                                               // string issuerAddress = "0xf827414cb1c39787d50bcebe534abe1ed2d5619f";//发起方的地址
                                                                               // var tempPrivateKey = "0x" + "3d3b37d27af1fe316f44913363e6edfa9fabf95f70f6e0754fdeedcd619ab651";
            var tempContractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var tempInputsParameters = new[] { BuildParams.CreateParam("address", "owner") };
            var tempParamsValue = new object[] { tempUserAddress };
            string tempFunctionName = "balanceOf";//调用合约方法

            //2、测试用户查询自己的账户余额
            ReceiptResultDto tempReceiptResultDto = await tempContractService.CallRequestAsync(contractAddress, abi, tempFunctionName, tempInputsParameters, tempParamsValue);
            var tempSolidityAbi = new SolidityABI(abi);
            var tempOutputList = tempSolidityAbi.OutputDecode(tempFunctionName, tempReceiptResultDto.Output);
            var tempBalance = tempOutputList[0].Result.ToString();
            Assert.Equal(997800, long.Parse(tempBalance));

        }


        /// <summary>
        /// 查询交易日志
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTransLogAysncTest()
        {
            string contractAddress = "0x36760f384f697707917fdb828e52e065a0619319";//上面测试部署合约得到合约地址
            string issuerAddress = "0xf827414cb1c39787d50bcebe534abe1ed2d5619f";//发起方的地址
            var tempUserAddress = "0x4d1a493a93effee00171b0b9c96fc3d62066febf";//接受转账的用户地址
            var tempPrivateKey = "0x" + "3d3b37d27af1fe316f44913363e6edfa9fabf95f70f6e0754fdeedcd619ab651";
            var tempContractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var tempInputsParameters = new[] { BuildParams.CreateParam("address", "account") };
            var tempParamsValue = new object[] { tempUserAddress };
            string tempFunctionName = "getTransLogs";//调用合约方法

            //2、测试查询交易
            ReceiptResultDto tempReceiptResultDto = await tempContractService.CallRequestAsync(contractAddress, abi, tempFunctionName, tempInputsParameters, tempParamsValue);
            var tempSolidityAbi = new SolidityABI(abi);

            var functionCallDecoder = new FunctionCallDecoder();
            //最快获取解析返回值
            var rs = functionCallDecoder.DecodeFunctionOutput<FunctionMultipleInputOutput>(tempReceiptResultDto.Output);

            var tempOutputList = tempSolidityAbi.OutputDecode(tempFunctionName, tempReceiptResultDto.Output).ToArray();
            //var tm = tempParameterDecoder.DecodeOutput(tempReceiptResultDto.Output, tempOutputList);
            var sendList = tempOutputList[0].Result.ToJson().ToObject<List<string>>();
            var receiveList = tempOutputList[1].Result.ToJson().ToObject<List<string>>();
            var typeList = tempOutputList[2].Result.ToJson().ToObject<List<Byte[]>>();
            var tempTypeList = new List<String>();

            foreach (var i in typeList)
            {
                //forearch的陷阱如果不复制给一个值，最后存进去都是同一个对象，循环尽量用for
                var tempData = i;
                var bytesType = ABIType.CreateABIType("bytes32");
                //when
                var typeString = bytesType.Decode<string>(i);
                tempTypeList.Add(typeString);
                //tempTypeList.Add(System.Text.Encoding.UTF8.GetString(tempData));
            }

            var amountList = tempOutputList[3].Result.ToJson().ToObject<List<string>>();

            var arrayType = ArrayType.CreateABIType("uint[]");


            //var result = bytesType.Decode<Guid>(encoded);
            Assert.True(sendList.Count > 0);
            Assert.True(receiveList.Count > 0);
            Assert.True(tempTypeList.Count > 0);
            Assert.True(amountList.Count > 0);
            // //sAssert.Equal(997800, long.Parse(tempBalance));

        }


    }
    [Function("getTransLogs")]
    [FunctionOutput]
    public class FunctionMultipleInputOutput
    {
        [Parameter("address[]")]
        public List<string> SendList { get; set; }

        [Parameter("address[]", "", 2)]
        public List<string> ReceiveList { get; set; }
        [Parameter("bytes32[]", 3)]
        public List<string> TypeNameList { get; set; }

        [Parameter("uint256[]", 4)]
        public List<BigInteger> AmountList { get; set; }


    }
}
