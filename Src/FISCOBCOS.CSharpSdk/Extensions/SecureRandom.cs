using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FISCOBCOS.CSharpSdk.Extensions
{
    public class SecureRandom : IRandom
    {
        private static readonly Org.BouncyCastle.Security.SecureRandom SecureRandomInstance =
            new Org.BouncyCastle.Security.SecureRandom();

        public void GetBytes(byte[] output)
        {
            SecureRandomInstance.NextBytes(output);
        }

        public void GetBytes(Span<byte> output)
        {
            SecureRandomInstance.NextBytes(output);
        }
    }
}
