using FISCOBCOS.CSharpSdk.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace FISCOBCOS.CSharpSdk.IService
{
    public interface IThReadService
    {
        void GetBlockInfo(long startBlockNumber, long stopBlockNumber, string contractAddress, Action<BlockByNumberDto> callBack = null);
      


    }
}
