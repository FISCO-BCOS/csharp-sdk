using FISCOBCOS.CSharpSdk;
using FISCOBCOS.CSharpSdk.ApiService;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FISCOBCOS.CSharpSdkTest
{
    /// <summary>
    /// json RPC 接口单元测试
    /// </summary>
    public class QueryApiServiceTest
    {
        public QueryApiService rpcApiService;
        public QueryApiServiceTest()
        {
            rpcApiService = new QueryApiService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId);
        }
        /// <summary>
        ///同步 json rpc api GetClientVersion测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GetApiClientVersionTest()
        {

            var clientVersionResult = rpcApiService.SendQuery<object>(JsonRPCAPIConfig.GetClientVersion, new { });
            var clientVersion = clientVersionResult.ToJson().ToJObject();

            //{"Build Time":"20190923 13:22:09","Build Type":"Linux/clang/Release","Chain Id":"1","FISCO-BCOS Version":"2.1.0","Git Branch":"HEAD","Git Commit Hash":"cb68124d4fbf3df563a57dfff5f0c6eedc1419cc","Supported Version":"2.1.0"}

            var chainId = clientVersion.GetValueByKey<string>("Chain Id");
            Assert.Equal("1", chainId);
            Assert.Equal("Linux/clang/Release", clientVersion.GetValueByKey<string>("Build Type"));
            Assert.Equal("2.1.0", clientVersion.GetValueByKey<string>("FISCO-BCOS Version"));
            Assert.Equal("2.1.0", clientVersion.GetValueByKey<string>("Supported Version"));

        }

        #region  异步请求查询测试
        /// <summary>
        ///异步 json rpc api GetClientVersion测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetApiClientVersionAsyncTest()
        {

            var clientVersionResult = await rpcApiService.SendQueryAsync<object>(JsonRPCAPIConfig.GetClientVersion, new { });
            var clientVersion = clientVersionResult.ToJson().ToJObject();

            //{"Build Time":"20190923 13:22:09","Build Type":"Linux/clang/Release","Chain Id":"1","FISCO-BCOS Version":"2.1.0","Git Branch":"HEAD","Git Commit Hash":"cb68124d4fbf3df563a57dfff5f0c6eedc1419cc","Supported Version":"2.1.0"}

            var chainId = clientVersion.GetValueByKey<string>("Chain Id");
            Assert.Equal("1", chainId);
            Assert.Equal("Linux/clang/Release", clientVersion.GetValueByKey<string>("Build Type"));
            Assert.Equal("2.1.0", clientVersion.GetValueByKey<string>("FISCO-BCOS Version"));
            Assert.Equal("2.1.0", clientVersion.GetValueByKey<string>("Supported Version"));

        }


        /// <summary>
        /// 异步 GetNodeIDList测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNodeIDListAsyncTest()
        {
            var result = await rpcApiService.SendQueryAsync<JToken>(JsonRPCAPIConfig.GetNodeIDList, BaseConfig.DefaultGroupId);
            var list = result.ToObject<List<string>>();
            Assert.True(list.Count == 4);
        }

        /// <summary>
        ///异步 GetPbftView测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPbftViewAsyncTest()
        {
            var result = await rpcApiService.SendQueryAsync<JToken>(JsonRPCAPIConfig.GetPbftView, BaseConfig.DefaultGroupId);
            var data = result.ToObject<HexBigInteger>();
            Assert.NotNull(data.HexValue);
        }

        /// <summary>
        ///异步 GetPeers测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPeersAsyncTest()
        {
            var result = await rpcApiService.SendQueryAsync<JToken>(JsonRPCAPIConfig.GetPeers, BaseConfig.DefaultGroupId);
            var data = result.ToObject<List<PeerDto>>();
            Assert.True(data.Count > 0);
            Assert.Equal("34b0f12f36ce7073d760f8ff7f16b0ef5ff3067e6b1e9c0239dad43c95eb6de0776e81abf8460c4add9cc0e37ef7fec08654cfe1d3a520b64f3503ba867faf55", data[0].NodeID);
        } 
        #endregion

        public class PeerDto
        {
            /// <summary>
            /// 
            /// </summary>
            public string Agency { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string IPAndPort { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Node { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string NodeID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> Topic { get; set; }
        }

    }
}
