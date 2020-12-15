using FISOBCOS_NetSdk.Dto;
using FISOBCOS_NetSdk.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FISOBCOS_NetSdk
{
    public class ContractService
    {

        public RpcClient rpcClient;

        /// <summary>
        /// 创建RPCClient
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns>返回一个RPC实例</returns>
        public ContractService(string url)
        {
            rpcClient = new RpcClient(new Uri(url));
        }

        /// <summary>
        /// 默认合约部署
        /// </summary>
        /// <param name="abi">合约abi</param>
        /// <param name="binCode">合约内容</param>
        /// <param name="privateKey">用户私钥</param>
        /// <returns>返回合约结果</returns>
        public async Task<string> DefaultDeployContract(string binCode, string privateKey)
        {
            var response = await DeployContract(binCode, privateKey, BaseConfig.DefaultRpcId, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId);
            return response;
        }

        /// <summary>
        /// 合约部署
        /// </summary>
        /// <param name="abi">合约abi</param>
        /// <param name="binCode">合约内容</param>
        /// <param name="privateKey">用户私钥</param>
        /// <param name="rpcId">rpc标示id</param>
        /// <param name="fiscoChainId">链id</param>
        /// <param name="groupId">群组id</param>
        /// <returns>部署合约结果</returns>
        public async Task<string> DeployContract(string binCode, string privateKey, int rpcId, int fiscoChainId, int groupId)
        {
            var blockNumber = await GetBlockNumber(rpcId, groupId);
            var transParams = BuildTransactionParams(binCode, blockNumber, "", fiscoChainId, groupId);
            var tx = BuildRLPTranscation(transParams);
            tx.Sign(new EthECKey(privateKey.HexToByteArray(), true));
            var rlpSignedEncoded = RLPEncoder.EncodeSigned(new SignedData(tx.Data, tx.Signature), 10).ToHex();
            var request = new RpcRequest(1, JsonRPCAPIConfig.SendRawTransaction, new object[] { 1, rlpSignedEncoded });
            var response = await rpcClient.SendRequestAsync<string>(request);
            return response;
        }

        /// <summary>
        /// 构建默认交易参数
        /// </summary>
        /// <param name="txData">交易数据</param>
        /// <param name="blockNumber">区块高度</param>
        /// <param name="to">发送交易地址，部署合约时候为空</param>
        /// <returns>交易参数对象 TransactionDto</returns>
        public TransactionDto BuildDefaultTransactionParams(string txData, long blockNumber, string to)
        {
            return BuildTransactionParams(txData, blockNumber, to, BaseConfig.DefaultChainId, BaseConfig.DefaultGroupId);
        }

        /// <summary>
        /// 构建交易参数
        /// </summary>
        /// <param name="txData">交易数据</param>
        /// <param name="blockNumber"></param>
        /// <param name="to">发送地址</param>
        /// <param name="fiscoChainId">链Id</param>
        /// <param name="groupId">群组Id</param>
        /// <returns>交易参数实体</returns>
        public TransactionDto BuildTransactionParams(string txData, long blockNumber, string to, int fiscoChainId, int groupId)
        {
            TransactionDto rawTransaction = new TransactionDto();

            rawTransaction.BlockLimit = blockNumber + 500;//交易防重上限，默认加500
            rawTransaction.Data = txData;//交易数据
            rawTransaction.ExtraData = "";//附加数据，默认为空字符串
            rawTransaction.FiscoChainId = fiscoChainId;//链ID
            rawTransaction.GasLimit = BaseConfig.DefaultGasLimit;//交易消耗gas上限，默认为30000000
            rawTransaction.GasPrice = BaseConfig.DefaultGasPrice;//默认为30000000
            rawTransaction.Randomid = new Random().Next(10000000, 1000000000); ;
            rawTransaction.To = to;//合约部署默认为空
            rawTransaction.Value = 0;//默认为0
            rawTransaction.GroupId = BaseConfig.DefaultGroupId;//群组ID 

            return rawTransaction;

        }

        /// <summary>
        /// 创建交易RLP
        /// </summary>
        /// <param name="rawTransaction">交易实体</param>
        /// <returns>RLPSigner</returns>
        public RLPSigner BuildRLPTranscation(TransactionDto rawTransaction)
        {
            var tx = new RLPSigner(new[] {rawTransaction.Randomid.ToBytesForRLPEncoding(),rawTransaction.GasPrice.ToBytesForRLPEncoding(), rawTransaction.GasLimit.ToBytesForRLPEncoding(),rawTransaction.BlockLimit.ToBytesForRLPEncoding(), rawTransaction.To.HexToByteArray(),  rawTransaction.Value.ToBytesForRLPEncoding(),rawTransaction.Data.HexToByteArray(),rawTransaction.FiscoChainId.ToBytesForRLPEncoding(),
              rawTransaction.GroupId.ToBytesForRLPEncoding(),rawTransaction.ExtraData.HexToByteArray()});
            return tx;
        }

        /// <summary>
        /// 获取当前区块高度
        /// </summary>
        /// <param name="rpcId">rpcId</param>
        /// <param name="groupId">群组Id</param>
        /// <returns>当前区块高度</returns>
        public async Task<long> GetBlockNumber(int rpcId, int groupId)
        {
            var request = new RpcRequest(rpcId, JsonRPCAPIConfig.GetBlockNumber, new object[] { groupId });
            var responseResult = await rpcClient.SendRequestAsync<string>(request);
            long blockNumber = Convert.ToInt64(responseResult, 16);
            return blockNumber;
        }
    }
}
