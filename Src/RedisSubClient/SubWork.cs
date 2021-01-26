using FISCOBCOS.CSharpSdk.Dto;
using FISCOBCOS.CSharpSdk.Utis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RedisSubClient
{
    public class SubWork
    {
        private SocketManager mgr;
        private ConnectionMultiplexer connection;
        private IDatabase db;
        public void Init()
        {
            var options = ConfigurationOptions.Parse("127.0.0.1:6379");
            options.Password = "";
            connection = ConnectionMultiplexer.Connect(options);
            db = connection.GetDatabase(1);//订阅存储地方的数据
            ISubscriber sub = connection.GetSubscriber();

            sub.Subscribe("RedisTranscationInfoSub", (channel, message) =>
            {
                var localProcess = Process.GetCurrentProcess();
                var data = message.ToString().ToObject<TransactionsItemDto>();
                db.HashSet(new RedisKey($"{localProcess.Id}/{localProcess.ProcessName}"), data.Hash, data.ToJson());
                //输出收到的消息
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 订阅获取得到的交易Hash： {data.Hash}");
            });

            Console.ReadKey();
        }
    }
}
