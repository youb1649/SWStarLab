using System;
using System.Net;
using System.Runtime.InteropServices;

namespace MNF
{
    public enum ENUM_UDP_CONTROL_FLAG
	{
        UDP_CONTROL_FLAG_UNKNOWN = 0,
        UDP_CONTROL_FLAG_SYN = 1,
        UDP_CONTROL_FLAG_FIN = 2,
		UDP_CONTROL_FLAG_ACK = 4,
		UDP_CONTROL_FLAG_PSH = 8,
	}

    public enum ENUM_UDP_DATAGRAM_TYPE
    {
        UDP_DATAGRAM_TYPE_UNKNOWN       = 0,
        UDP_DATAGRAM_TYPE_UNRELIABLE    = 1,
        UDP_DATAGRAM_TYPE_RELIABLE      = 2,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class UdpDatagramHeader
    {
        static UdpDatagramHeader emptyMessageHeader = new UdpDatagramHeader();
        static int headerSize = MarshalHelper.GetManagedDataSize(typeof(UdpDatagramHeader));

        // for udp datagram
        [MarshalAs(UnmanagedType.U1)]
        public byte datagramType;
		[MarshalAs(UnmanagedType.U1)]
		public byte controlbit;
        
        // for reliable, unreliable
        [MarshalAs(UnmanagedType.I4)]
        public int seqNumber;
        [MarshalAs(UnmanagedType.I8)]
        public long sendTick;

        // for reliable
        [MarshalAs(UnmanagedType.U2)]
        public ushort sendSize;
        [MarshalAs(UnmanagedType.U2)]
        public ushort window;
        [MarshalAs(UnmanagedType.I4)]
        public int datagramSequence;

        [MarshalAs(UnmanagedType.U4)]
        public uint sessionUID;

        public UdpDatagramHeader()
        {
            initHeader();
        }

        public void initHeader()
        {
            sessionUID = 0;
            datagramType = (byte)ENUM_UDP_DATAGRAM_TYPE.UDP_DATAGRAM_TYPE_UNKNOWN;
            controlbit = 0;
            seqNumber = 0;
            sendTick = 0;
            sendSize = 0;
            window = 0;
            datagramSequence = 0;
        }

        public static int getHeaderSize()
        {
            return headerSize;
        }

        public static Type getHeaderType()
        {
            return emptyMessageHeader.GetType();
        }

        public void setUnreliable()
        {
            datagramType = (byte)ENUM_UDP_DATAGRAM_TYPE.UDP_DATAGRAM_TYPE_UNRELIABLE;
            controlbit = (byte)ENUM_UDP_CONTROL_FLAG.UDP_CONTROL_FLAG_UNKNOWN;
        }

        private void setReliable()
        {
            datagramType = (byte)ENUM_UDP_DATAGRAM_TYPE.UDP_DATAGRAM_TYPE_RELIABLE;
            controlbit = (byte)ENUM_UDP_CONTROL_FLAG.UDP_CONTROL_FLAG_UNKNOWN;
        }

        public void setControlBit_PSH()
        {
            setReliable();
            controlbit += (byte)ENUM_UDP_CONTROL_FLAG.UDP_CONTROL_FLAG_PSH;
        }

        public void setControlBit_ACK()
        {
            setReliable();
            controlbit += (byte)ENUM_UDP_CONTROL_FLAG.UDP_CONTROL_FLAG_ACK;
        }

        static public bool compare(UdpDatagramHeader a, UdpDatagramHeader b)
        {
            if ((a.sessionUID != b.sessionUID)
            || (a.datagramType != b.datagramType)
            || (a.controlbit != b.controlbit)
            || (a.seqNumber != b.seqNumber)
            || (a.sendTick != b.sendTick)
            || (a.sendSize != b.sendSize)
            || (a.window != b.window))
                return false;

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class UdpMessageHeader
    {
        private static UdpMessageHeader emptyMessageHeader = new UdpMessageHeader();
        private static int headerSize = MarshalHelper.GetManagedDataSize(typeof(UdpMessageHeader));

        [MarshalAs(UnmanagedType.I2)]
        public short messageSize;
        [MarshalAs(UnmanagedType.U2)]
        public ushort messageID;

        public UdpMessageHeader()
        {
            messageSize = 0;
            messageID = 0;
        }

        public static int getHeaderSize()
        {
            return headerSize;
        }

        public static Type getHeaderType()
        {
            return emptyMessageHeader.GetType();
		}
    }

    public class UdpMessageInfo
    {
        public object UdpMessage { get; set; }
        public EndPoint RemoteIP { get; set; }
        public bool IsSendMe { get; set; }

        public UdpMessageInfo()
        {
            UdpMessage = null;
            RemoteIP = null;
            IsSendMe = false;
        }
    }
}
