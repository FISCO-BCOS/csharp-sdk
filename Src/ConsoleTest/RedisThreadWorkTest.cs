using FISCOBCOS.CSharpSdk;
using FISCOBCOS.CSharpSdk.ApiService;
using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.IService;
using FISCOBCOS.CSharpSdk.Utis;
using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using StackExchange.Redis;

namespace ConsoleTest
{
    public class RedisThreadWorkTest
    {

        IThReadService _thReadService;

        private SocketManager mgr;
        private ConnectionMultiplexer connection;
        private IDatabase db;
        ISubscriber sub;
        public void WorkStart()
        {
            string contractAddress = "0xf6b77b311ca2b59a63397f243952950bb69012a5";//指定合约
            var options = ConfigurationOptions.Parse("127.0.0.1:6379");
            options.Password = "";
            connection = ConnectionMultiplexer.Connect(options);
            db = connection.GetDatabase(2);
             sub = connection.GetSubscriber();
            _thReadService = new ThreadService(BaseConfig.DefaultUrl, BaseConfig.DefaultRpcId, BaseConfig.DefaultGroupId);
            _thReadService.GetBlockInfo(35000, 35021, contractAddress, StorageData);

        }

        public void StorageData(BlockByNumberDto input)
        {
            if (input != null)
            {
                db.HashSet(new RedisKey("BlockNumberInfo"), long.Parse(new HexBigInteger(input.Number).Value.ToString()), input.ToJson());//将当前区块存入指定的数据库
                Console.WriteLine($"区块高度：{new HexBigInteger(input.Number).Value},存入区块数据为:{db.HashGet(new RedisKey("BlockNumberInfo"), long.Parse(new HexBigInteger(input.Number).Value.ToString())).ToString().ToObject<BlockByNumberDto>().Hash }");
                Console.WriteLine("-----------------------------------------------------------");
                Thread.Sleep(1000);
            }
           
            if (input.Transactions != null)
            {
                var tempList = input.Transactions.ToList();
                for (int i = 0; i < tempList.Count; i++)
                {
                    db.HashSet(new RedisKey("TranscationInfo"), tempList[i].Hash, tempList[i].ToJson());
                    Console.WriteLine($"存入交易数据为:{ db.HashGet(new RedisKey("TranscationInfo"), tempList[i].Hash)}");
                    Console.WriteLine("-----------------------------------------------------------");
                    if (sub != null)
                    { //发布订阅的pub
                        sub.Publish("RedisTranscationInfoSub", tempList[i].ToJson());
                    }
                    Thread.Sleep(1000);
                }
            }

           
        }
    }


}
