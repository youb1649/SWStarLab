using System.Runtime.InteropServices;

namespace MNF_Common
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CustomBinaryMessageHeader
    {
        [MarshalAs(UnmanagedType.I2)]
        public short messageSize;

        [MarshalAs(UnmanagedType.U2)]
        public ushort messageID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CustomJsonMessageHeader
    {
        [MarshalAs(UnmanagedType.I2)]
        public short messageSize;

        [MarshalAs(UnmanagedType.U2)]
		public ushort messageID;
    }
}