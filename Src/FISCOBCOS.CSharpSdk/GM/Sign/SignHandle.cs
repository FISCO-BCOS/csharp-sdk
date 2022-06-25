namespace FISCOBCOS.CSharpSdk.Sign
{
    public interface SignHandle
    {
        public byte[] Hash(byte[] msg);

        public byte[] Sign(byte[] digest);

        public bool Verify(byte[] sign, byte[] digest);
    }
}