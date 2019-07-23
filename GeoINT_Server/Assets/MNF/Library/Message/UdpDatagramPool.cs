using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;

namespace MNF
{
	static class UdpMessageBuffer
	{
		public static int MaxMessageSize()
		{
			return 1024 * 32;
		}
	}

    public class UdpDatagram
    {
        protected UdpDatagramHeader udpDatagramHeader;

        public byte[] SerializedBuffer { get; protected set; }
        public int SerializedSize { get; protected set; }
        public UdpSession LinkedSession { get; set; }
        public UdpDatagramHeader UdpDatagramHeader { get { return udpDatagramHeader; } protected set { udpDatagramHeader = value; } }

        public static int HeaderSerializedSize { get { return UdpDatagramHeader.getHeaderSize(); } }
        public static int SerializedBufferLength { get { return 1024; } }

        public UdpDatagram(UdpSession session)
        {
            this.udpDatagramHeader = new UdpDatagramHeader();
            SerializedBuffer = new byte[SerializedBufferLength];
            LinkedSession = session;
        }
    }

    public class UdpDatagramSend : UdpDatagram
    {
        public bool IsSending { get; set; }
		public IPEndPoint EndPoint { get; set; }

        public UdpDatagramSend(UdpSession session) : base(session)
        {
            IsSending = false;
        }

        public void serializeHeader()
        {
            Debug.Assert(UdpDatagramHeader.sendTick > 0);

            int serializedSize = 0;
            MarshalHelper.RawSerialize(udpDatagramHeader, SerializedBuffer, 0, ref serializedSize);
            SerializedSize = HeaderSerializedSize + udpDatagramHeader.sendSize;
        }

        public void serializeDatagram<T>(ref T managedData)
        {
            int serializedSize = 0;
            MarshalHelper.RawSerialize(udpDatagramHeader, SerializedBuffer, 0, ref serializedSize);
            SerializedSize = serializedSize;
            MarshalHelper.RawSerialize(managedData, SerializedBuffer, serializedSize, ref serializedSize);
            SerializedSize += serializedSize;
        }
    }

    public class UdpDatagramRecv : UdpDatagram
    {
        public IntPtr marshalAllocatedBuffer;
        public int marshalAllocatedBufferSize;

        public UdpDatagramRecv(UdpSession linkedSession) : base(linkedSession)
        {
            marshalAllocatedBufferSize = UdpMessageBuffer.MaxMessageSize();
            marshalAllocatedBuffer = MarshalHelper.AllocGlobalHeap(marshalAllocatedBufferSize);
        }

        ~UdpDatagramRecv()
        {
            MarshalHelper.DeAllocGlobalHeap(marshalAllocatedBuffer);
        }

        public void deSerializeUdpDatagramHeader()
        {
            udpDatagramHeader = (UdpDatagramHeader)MarshalHelper.RawDeSerialize(
                SerializedBuffer
                , UdpDatagramHeader.getHeaderType()
                , 0
                , ref marshalAllocatedBuffer
                , ref marshalAllocatedBufferSize);
        }
    }

    public class UdpDatagramSendPool
    {
        object udpDatagramPoolLock;
        LinkedList<UdpDatagramSend> udpDatagramPool;

        public int Count { get { return udpDatagramPool.Count; } }

        public UdpDatagramSendPool()
        {
            udpDatagramPoolLock = new object();
            udpDatagramPool = new LinkedList<UdpDatagramSend>();
        }

        public UdpDatagramSend alloc(UdpSession session)
        {
            UdpDatagramSend udpDatagram = null;
            lock (udpDatagramPoolLock)
            {
                if (udpDatagramPool.Count > 0)
                {
                    udpDatagram = udpDatagramPool.First.Value;
                    Debug.Assert(udpDatagram.LinkedSession == null);
                    Debug.Assert(udpDatagram.IsSending == false);

                    udpDatagram.LinkedSession = session;
                    udpDatagramPool.RemoveFirst();
                }
                else
                {
                    udpDatagram = new UdpDatagramSend(session);
                }
            }
            return udpDatagram;
        }

        public void free(UdpDatagramSend udpDatagram)
        {
            Debug.Assert(udpDatagram  != null);

            lock (udpDatagramPoolLock)
            {
                Debug.Assert(udpDatagram.LinkedSession != null);
                Debug.Assert(udpDatagram.IsSending == false);

                udpDatagram.UdpDatagramHeader.initHeader();
                udpDatagram.LinkedSession = null;
                udpDatagramPool.AddLast(udpDatagram);
            }
        }
    }

    public class UdpDatagramRecvPool
    {
        object udpDatagramPoolLock;
        LinkedList<UdpDatagramRecv> udpDatagramPool;

        public int Count { get { return udpDatagramPool.Count; } }

        public UdpDatagramRecvPool()
        {
            udpDatagramPoolLock = new object();
            udpDatagramPool = new LinkedList<UdpDatagramRecv>();
        }

        public UdpDatagramRecv alloc(UdpSession linkedSession)
        {
            UdpDatagramRecv udpDatagram = null;
            lock (udpDatagramPoolLock)
            {
                if (udpDatagramPool.Count > 0)
                {
                    udpDatagram = udpDatagramPool.First.Value;
                    Debug.Assert(udpDatagram.LinkedSession == null);

                    udpDatagram.UdpDatagramHeader.initHeader();
                    udpDatagram.LinkedSession = linkedSession;
                    udpDatagramPool.RemoveFirst();
                }
                else
                {
                    udpDatagram = new UdpDatagramRecv(linkedSession);
                }
            }
            return udpDatagram;
        }

        public void free(UdpDatagramRecv udpDatagram)
        {
            if (udpDatagram == null)
                return;

            lock (udpDatagramPoolLock)
            {
                udpDatagram.LinkedSession = null;
                udpDatagramPool.AddLast(udpDatagram);
            }
        }
    }
}
