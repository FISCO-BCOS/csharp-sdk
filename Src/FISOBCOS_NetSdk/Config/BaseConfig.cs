using System;
using System.Collections.Generic;
using System.Text;

namespace FISOBCOS_NetSdk
{
    public class BaseConfig
    {
        /// <summary>
        /// 链上jsonapi 通信地址 通常是ip：8545端口
        /// </summary>
        public static string DefaultUrl = "http://119.27.161.250:8545";
       
        /// <summary>
        /// 默认链Id
        /// </summary>
        public static int DefaultChainId = 1;

        /// <summary>
        /// 默认群组Id
        /// </summary>
        public static int DefaultGroupId = 1;

        /// <summary>
        /// rpc 请求默认标识Id
        /// </summary>
        public static int DefaultRpcId = 1;
        /// <summary>
        /// 默认GasLimit
        /// </summary>
        public static int DefaultGasLimit = 30000000;

        /// <summary>
        /// 默认GasPrice
        /// </summary>
        public static int DefaultGasPrice = 30000000;


    }
}
