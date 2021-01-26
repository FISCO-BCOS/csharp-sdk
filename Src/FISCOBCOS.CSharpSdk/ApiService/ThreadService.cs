using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.IService;
using FISCOBCOS.CSharpSdk.Utils;
using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FISCOBCOS.CSharpSdk.ApiService
{
    public class ThreadService : IThReadService
    {
        public QueryApiService _rpcApiService;
        public int _groupId;
        public long _index;
        public ThreadService(string url, int rpcId, int groupId)
        {
            _rpcApiService = new QueryApiService(url, rpcId);
            _groupId = groupId;
        }

        public void GetBlockInfo(long startBlockNumber = 1, long stopBlockNumber = 1, string contractAddress = "", Action<BlockByNumberDto> callBack = null)
        {
            new ThreadPoolWorkUtils().ThreadPoolWork(x =>
            {

                while (true)
                {
                    long newestBlockNumber = _rpcApiService.GetBlockNumber();//获取最新区块高度
                    if (stopBlockNumber > newestBlockNumber)
                        stopBlockNumber = newestBlockNumber;

                    if (startBlockNumber <= newestBlockNumber && startBlockNumber <= stopBlockNumber)
                    {
                        HexBigInteger blockNumber = new HexBigInteger(startBlockNumber);
                        bool state = true;
                        var result = _rpcApiService.SendQuery<BlockByNumberDto>(JsonRPCAPIConfig.GetBlockByNumber, new object[] { _groupId, blockNumber.ToString(), state });
                        if (!string.IsNullOrWhiteSpace(contractAddress))
                        {//获取指定的合约交易区块链数据
                            result.Transactions = result.Transactions.Where(x => x.To == contractAddress).ToList();
                        }
                        callBack?.Invoke(result);//指定回调进行操作
                        startBlockNumber++;
                    }

                }
            });
        }
    }
}
