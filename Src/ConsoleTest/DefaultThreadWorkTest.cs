using FISCOBCOS.CSharpSdk;
using FISCOBCOS.CSharpSdk.ApiService;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.IService;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleTest
{
    public class DefaultThreadWorkTest
    {

        public Queue<Object> _blockInfoQueue;
        public Queue<Object> _transcationInfoQueue;
        IThReadService _thReadService;
        public void WorkStart()
        {

            _blockInfoQueue = new Queue<object>();
            _transcationInfoQueue = new Queue<object>();
            _thReadService = new ThreadService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultGroupId);
            _thReadService.GetBlockInfo(35000, 35021, "", StorageData);

        }

        public void StorageData(BlockByNumberDto input)
        {

            if (input != null)
            {
                lock (_blockInfoQueue)
                {
                    _blockInfoQueue.Enqueue(input);
                    Console.WriteLine($"区块高度：{new HexBigInteger(input.Number).Value},存入区块数据为:{ _blockInfoQueue.Dequeue().ToString().ToObject<BlockByNumberDto>().Hash}");
                    Console.WriteLine("-----------------------------------------------------------");
                    Thread.Sleep(1000);
                }
                if (input.Transactions != null)
                {
                    var tempList = input.Transactions.ToList();
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        lock (_transcationInfoQueue)
                        {
                            _transcationInfoQueue.Enqueue(tempList[i]);
                            Console.WriteLine($"存入交易数据为:{ _transcationInfoQueue.Dequeue().ToJson()}");
                            Console.WriteLine("-----------------------------------------------------------");
                            Thread.Sleep(1000);
                        }
                    }
                }
            }

           
        }
    }
}
