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
            bool getAbiState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\abiSol\\bin\\" + "QTest.abi", out abi);
            bool getBinCodeState = FileUtils.ReadFile(Environment.CurrentDirectory + "\\TestData\\abiSol\\bin\\" + "QTest.bin", out binCode);

        }

        /// <summary>
        /// 从链上获取对应的abi编码进行解码（单个参数解码）
        /// </summary>
        [Fact]
        public void GetAbiEncodeTest()
        {
            //var data = "0x2dc9".HexToBigInteger(true);
            var contractService = new ContractService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId, privateKey);

            string contractAddress = "0x23fe092ebbbff8fede5e20e6b96dc0b11a98af8f";//上面测试部署合约得到合约地址
            string functionName = "test";//调用合约方法

            ReceiptResultDto receiptResultDto = contractService.CallRequest(contractAddress, abi, functionName);
            Assert.NotEmpty(receiptResultDto.Output);
            var solidityAbi = new SolidityABI(abi);
            var outputList = solidityAbi.OutputDecode(functionName, receiptResultDto.Output);
            var tempData = (byte[])outputList[0].Result;
            var bytesType = ABIType.CreateABIType("bytes32");//原有编码得到数据类型，
            var result = bytesType.Decode<BigInteger>(tempData).ToString();//需要转化的数据

            Assert.Equal("1", result);
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
           var data=(TestParamsInput)parameterDecoder.DecodeAttributes(result2,typeof(TestParamsInput));

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
