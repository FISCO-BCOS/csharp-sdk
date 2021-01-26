using System;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //new DefaultThreadWorkTest().WorkStart();//默认内存操作
            new RedisThreadWorkTest().WorkStart();//redis 存储操作
            Console.ReadLine();
        }
    }
}
