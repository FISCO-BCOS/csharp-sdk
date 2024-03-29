﻿using FISCOBCOS.CSharpSdk.Core;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Utils;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.JsonDeserialisation;
using Nethereum.ABI.Model;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.RLP;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FISCOBCOS.CSharpSdk
{
    public class ContractService : BaseService
    {

        public RpcClient _rpcClient;

        /// <summary>
        /// 创建合约服务
        /// </summary>
        /// <param name="url">请求底层通信地址</param>
        /// <param name="rpcId">rpcId 默认为1</param>
        /// <param name="chainId">链Id</param>
        /// <param name="groupId">群组Id</param>
        /// <param name="privateKey">用私钥</param>
        public ContractService(string url, int rpcId, int chainId, int groupId, string privateKey) : base(url, rpcId, chainId, groupId, privateKey)
        {
            _rpcClient = new RpcClient(new Uri(url));

        }

        /// <summary>
        /// 异步 通用合约部署，只返回交易Hash
        /// </summary>
        /// <param name="binCode">合约内容</param>
        /// <returns>交易Hash</returns>
        public async Task<string> DeployContractAsync(string binCode, string abi = null, params object[] values)
        {
            var blockNumber = await GetBlockNumberAsync();
            var resultData = "";
            var rlpTransData = BuildDeployContractData(binCode, abi, blockNumber, values);

            string txHash = null;
            if (BaseConfig.IsSMCrypt)//国密版本
            {
                var txs = GMGetTransRlp(rlpTransData);
                //国密请求
                txHash =await GMSendRequestAysnc<string>(txs);
            }
            else
            {//标准版本
                rlpTransData.Sign(new EthECKey(this._privateKey.HexToByteArray(), true));
                txHash = await SendRequestAysnc<string>(rlpTransData.Data, rlpTransData.Signature);

            }
            return txHash;

        }

        /// <summary>
        /// 异步 通用合约部署，返回交易回执
        /// </summary>
        /// <param name="binCode">合约内容</param>
        /// <returns>交易回执</returns>
        public async Task<ReceiptResultDto> DeployContractWithReceiptAsync(string binCode, string abi = null, params object[] values)
        {
            var txHash = await DeployContractAsync(binCode, abi, values);
            var receiptResult = await GetTranscationReceiptAsync(txHash);
            return receiptResult;
        }



        /// <summary>
        ///异步 发送交易,返回交易回执
        /// </summary>
        /// <param name="abi">合约abi</param>
        /// <param name="contractAddress">合约地址</param>
        /// <param name="functionName">合约请求调用方法名称</param>
        /// <param name="inputsParameters">方法对应的 参数</param>
        /// <param name="value">请求参数值</param>
        /// <returns>交易回执</returns>
        public async Task<ReceiptResultDto> SendTranscationWithReceiptAsync(string abi, string contractAddress, string functionName, Parameter[] inputsParameters, params object[] value)
        {
            ReceiptResultDto receiptResult = new ReceiptResultDto();
            var blockNumber =await GetBlockNumberAsync();
            var rlpTransData = BuildTransData(abi, contractAddress, functionName, blockNumber, inputsParameters, value);
            string txHash = null;
            if (BaseConfig.IsSMCrypt)//国密版本
            {
                var txs = GMGetTransRlp(rlpTransData);
                //国密请求
                txHash = await GMSendRequestAysnc<string>(txs);
            }
            else
            {//标准版本
                rlpTransData.Sign(new EthECKey(this._privateKey.HexToByteArray(), true));
                 txHash = await SendRequestAysnc<string>(rlpTransData.Data, rlpTransData.Signature);
            }

            if (txHash != null)
            {
                receiptResult = await GetTranscationReceiptAsync(txHash);
                if (receiptResult == null)
                    throw new Exception("txHash != null 的时候报错了：" + receiptResult.ToJson());
            }
            return receiptResult;
        }

        /// <summary>
        ///异步 发送交易,返回交易回执
        /// </summary>
        /// <param name="abi">合约abi</param>
        /// <param name="contractAddress">合约地址</param>
        /// <param name="functionName">合约请求调用方法名称</param>
        /// <param name="inputsParameters">方法对应的 参数</param>
        /// <param name="value">请求参数值</param>
        /// <returns>交易回执</returns>
        public async Task<string> SendTranscationWithTransHashAsync(string abi, string contractAddress, string functionName, Parameter[] inputsParameters, params object[] value)
        {
            ReceiptResultDto receiptResult = new ReceiptResultDto();
            var blockNumber = await GetBlockNumberAsync();
            var rlpTransData = BuildTransData(abi, contractAddress, functionName,blockNumber, inputsParameters, value);
            string txHash = null;
            if (BaseConfig.IsSMCrypt)//国密版本
            {
                var txs = GMGetTransRlp(rlpTransData);
                //国密请求
                txHash = await GMSendRequestAysnc<string>(txs);
            }
            else
            {//标准版本
                rlpTransData.Sign(new EthECKey(this._privateKey.HexToByteArray(), true));
                txHash = await SendRequestAysnc<string>(rlpTransData.Data, rlpTransData.Signature);
            }

            return txHash;
        }


        /// <summary>
        /// 异步 获取交易回执
        /// </summary>
        /// <param name="tanscationHash">交易Hash</param>
        /// <returns></returns>
        public async Task<ReceiptResultDto> GetTranscationReceiptAsync(string tanscationHash)
        {
            var request = new RpcRequest(this._requestId, JsonRPCAPIConfig.GetTransactionReceipt, new object[] { this._requestObjectId, tanscationHash });
            var getRequest = new RpcRequestMessage(this._requestId, JsonRPCAPIConfig.GetTransactionReceipt, new object[] { this._requestObjectId, tanscationHash });
            //var result = await HttpUtils.RpcPost<ReceiptResultDto>(BaseConfig.DefaultUrl, getRequest);
            var result = await this._rpcClient.SendRequestAsync<ReceiptResultDto>(request);
            if (result == null) throw new Exception(" 获取交易回执方法报空：" + result.ToJson());
            return result;
        }

        /// <summary>
        /// 异步 Call 调用 适用于链上调用但不需要共识（通常用constant,view等修饰的合约方法）
        /// </summary>
        /// <param name="contractAddress">合约地址</param>
        /// <param name="abi">合约abi</param>
        /// <param name="callFunctionName">调用方法名称</param>
        /// <returns>返回交易回执</returns>
        public async Task<ReceiptResultDto> CallRequestAsync(string contractAddress, string abi, string callFunctionName, Parameter[] inputsParameters = null, params object[] value)
        {
            var callDto = BuildCallData(contractAddress, abi, callFunctionName, inputsParameters, value);
            var getRequest = new RpcRequest(this._requestId, JsonRPCAPIConfig.Call, new object[] { this._requestObjectId, callDto });
            var result = await this._rpcClient.SendRequestAsync<ReceiptResultDto>(getRequest);

            //var getRequest = new RpcRequestMessage(this.RequestId, JsonRPCAPIConfig.Call, new object[] { this.RequestObjectId, callDto });
            //var result = HttpUtils.RpcPost<ReceiptResultDto>(BaseConfig.DefaultUrl, getRequest); //同步方法
            return result;

        }

        /// <summary>
        /// 异步请求发送RPC交易
        /// </summary>
        /// <typeparam name="TResult">返回结果</typeparam>
        /// <param name="txData">交易数据（rlp）</param>
        /// <param name="txSignature">交易签名</param>
        /// <returns>返回交易结果</returns>
        protected async Task<TResult> SendRequestAysnc<TResult>(byte[][] txData, EthECDSASignature txSignature)
        {
            var rlpSignedEncoded = RLPEncoder.EncodeSigned(new SignedData(txData, txSignature), 10).ToHex();
            var request = new RpcRequest(this._requestId, JsonRPCAPIConfig.SendRawTransaction, new object[] { this._requestObjectId, rlpSignedEncoded });
            var response = await _rpcClient.SendRequestAsync<TResult>(request);
            return response;
        }


        /// <summary>
        /// 异步请求发送RPC交易
        /// </summary>
        /// <typeparam name="TResult">返回结果</typeparam>
        /// <param name="txData">交易数据（rlp）</param>
        /// <param name="txSignature">交易签名</param>
        /// <returns>返回交易结果</returns>
        protected async Task<TResult> GMSendRequestAysnc<TResult>(byte[] rsvData)
        {
            var rlpSignedEncoded = rsvData.ToHex();
            var request = new RpcRequest(this._requestId, JsonRPCAPIConfig.SendRawTransaction, new object[] { this._requestObjectId, rlpSignedEncoded });
            var response = await _rpcClient.SendRequestAsync<TResult>(request);
            return response;
        }
       
        /// <summary>
        /// 异步获取当前区块高度
        /// </summary>
        /// <param name="rpcId">rpcId</param>
        /// <param name="groupId">群组Id</param>
        /// <returns>当前区块高度</returns>
        public async Task<long> GetBlockNumberAsync()
        {
            var request = new RpcRequest(this._rpcId, JsonRPCAPIConfig.GetBlockNumber, new object[] { this._groupId });
            var responseResult = await _rpcClient.SendRequestAsync<string>(request);
            long blockNumber = Convert.ToInt64(responseResult, 16);
            return blockNumber;
        }
    }
}
