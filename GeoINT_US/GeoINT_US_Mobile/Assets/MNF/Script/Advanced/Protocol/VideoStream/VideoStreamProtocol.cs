using System.Runtime.InteropServices;

namespace MNF_Common
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VideoStreamMessageHeader
    {
        [MarshalAs(UnmanagedType.I4)]
        public int messageSize;

        [MarshalAs(UnmanagedType.U2)]
        public ushort messageID;
    }
}