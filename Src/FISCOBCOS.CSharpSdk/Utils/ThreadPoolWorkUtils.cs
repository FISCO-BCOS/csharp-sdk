using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FISCOBCOS.CSharpSdk.Utils
{
    public class ThreadPoolWorkUtils
    {
        public void ThreadPoolWork(WaitCallback callback)
        {
            ThreadPool.QueueUserWorkItem(callback);
        }
    }
}
