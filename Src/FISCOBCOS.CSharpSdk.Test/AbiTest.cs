using FISCOBCOS.CSharpSdk.Core;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Utils;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace FISCOBCOS.CSharpSdk.Test
{

    public class AbiTest
    {

        public string privateKey = "";
        string binCode = "";
        string abi = "";

        public AbiTest()
        {
            this.privateKey = "0x25aa95ed437f8efaf37cf849a5a6ba212308d5d735105e03e38410542bf1d5ff";
            //bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\abiSol\\bin\\" + "QTest.abi", out abi);
            //bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\abiSol\\bin\\" + "QTest.bin", out binCode);

            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\abiSol\\bin\\" + "AbiEncode.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\abiSol\\bin\\" + "AbiEncode.bin", out binCode);

        }


        /// <summary>
        ///同步部署合约，并得到交易回执，获得交易回执会出现有一定几率为空
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void DeployContractWithReceiptTest()
        {
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            var result = contractService.DeployContractWithReceipt(binCode, abi);
            Assert.NotNull(result.ContractAddress);//0xc8c8b76c4abc5618e37863e4328c0d46054e9f09
        }

        /// <summary>
        /// 从链上获取对应的abi编码进行解码（单个参数解码）
        /// </summary>
        [Fact]
        public void GetAbiEncodeTest()
        {
            //var data = "0x2dc9".HexToBigInteger(true);
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);

            string contractAddress = "0xc8c8b76c4abc5618e37863e4328c0d46054e9f09";//上面测试部署合约得到合约地址
            string functionName = "abiEndode";//调用合约方法

            var inputsParameters = new[] { BuildParams.CreateParam("uint", "a"), BuildParams.CreateParam("string", "b"), BuildParams.CreateParam("uint", "c") };
            var paramsValue = new object[] { 1, "abc", 123 };

            ReceiptResultDto receiptResultDto = contractService.CallRequest(contractAddress, abi, functionName, inputsParameters, paramsValue);
            Assert.NotEmpty(receiptResultDto.Output);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, receiptResultDto.Output);
            var tempData = (byte[])outputList[0].Result;
            var dataHex = tempData.ToHex();
            ParameterDecoder parameterDecoder = new ParameterDecoder();
            //可以解析得到数据
            var data = (AbiParamsInput)parameterDecoder.DecodeAttributes(tempData, typeof(AbiParamsInput));

            Assert.Equal(1, data.First);
            Assert.Equal("abc", data.Second);
            Assert.Equal(123, data.Third);
        }
        /// <summary>
        /// 调用链上abi decode
        /// </summary>
        [Fact]
        public void GetAbiDecodeTest()
        {

            string bytesData = "00000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000000000000000000000000000000000060000000000000000000000000000000000000000000000000000000000000007b00000000000000000000000000000000000000000000000000000000000000036162630000000000000000000000000000000000000000000000000000000000";

            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);
            string contractAddress = "0xc8c8b76c4abc5618e37863e4328c0d46054e9f09";//上面测试部署合约得到合约地址
            string functionName = "abiDecode";//调用合约方法
            var inputsParameters = new[] { BuildParams.CreateParam("bytes", "encodeData") };
            var paramsValue = new object[] { bytesData.HexToByteArray() };
            ReceiptResultDto receiptResultDto = contractService.CallRequest(contractAddress, abi, functionName, inputsParameters,paramsValue);
            Assert.NotEmpty(receiptResultDto.Output);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, receiptResultDto.Output);
            Assert.Equal(1, (BigInteger)outputList[0].Result);
            Assert.Equal("abc", (string)outputList[1].Result);
            Assert.Equal(123, (BigInteger)outputList[2].Result);
        }

        public class AbiParamsInput
        {
            [Parameter("uint", 1)]
            public BigInteger First { get; set; }
            [Parameter("string", 2)]
            public string Second { get; set; }
            [Parameter("uint", 3)]
            public BigInteger Third { get; set; }
        }


        /// <summary>
        ///abi 独立参数的编码解码 （多个参数解码）
        /// </summary>
        [Fact]
        public void AbiEncodeAndDecodeTest()
        {
            var abiEncode = new ABIEncode();
            var result1 = abiEncode.GetABIEncoded(new ABIValue("string", "hello"), new ABIValue("int", 69),
                new ABIValue("string", "world")).ToHex(true);
            var paramsEncoded =
               "0000000000000000000000000000000000000000000000000000000000000060000000000000000000000000000000000000000000000000000000000000004500000000000000000000000000000000000000000000000000000000000000a0000000000000000000000000000000000000000000000000000000000000000568656c6c6f0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005776f726c64000000000000000000000000000000000000000000000000000000";
            var result2 = abiEncode.GetABIParamsEncoded(new TestParamsInput() { First = "hello", Second = 69, Third = "world" });
            Assert.Equal("0x" + paramsEncoded, result2.ToHex(true));

            ParameterDecoder parameterDecoder = new ParameterDecoder();
            //可以解析得到数据
            var data = (TestParamsInput)parameterDecoder.DecodeAttributes(result2, typeof(TestParamsInput));

            Assert.Equal("hello", data.First);
            Assert.Equal(69, data.Second);
            Assert.Equal("world", data.Third);
        }

        public class TestParamsInput
        {
            [Parameter("string", 1)]
            public string First { get; set; }
            [Parameter("int256", 2)]
            public int Second { get; set; }
            [Parameter("string", 3)]
            public string Third { get; set; }
        }
    }
}
