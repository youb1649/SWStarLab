using System.Collections.Generic;
using System.Net;
using MNF.Message;

namespace MNF
{
    public class UdpSession : SessionBase
    {
        object LockTCB { get; set; }
        Dictionary<EndPoint, TCBforSACK> TCBrorSACKMap { get; set; }

        internal int UnreliableSequence { get; set; }
        internal UdpDatagramSendPool UdpDatagramSendPool { get; private set; }
        internal UdpDatagramRecvPool UdpDatagramRecvPool { get; private set; }

        internal override void Init()
        {
            RecvCircularBuffer = new CircularBuffer(UdpMessageBuffer.MaxMessageSize());
            UnreliableSequence = 0;
            LockTCB = new object();
            SessionType = SessionType.SESSION_UDP;
            TCBrorSACKMap = new Dictionary<EndPoint, TCBforSACK>();
            UdpDatagramSendPool = new UdpDatagramSendPool();
            UdpDatagramRecvPool = new UdpDatagramRecvPool();
        }

        public bool unreliableSendTo<T>(EndPoint ipEndPoint, ref T message)
        {
            return AsyncIO_Udp.UnreliableSendTo(this, ipEndPoint, ref message);
        }

        public bool reliableSendTo<T>(EndPoint ipEndPoint, ref T message)
        {
            return AsyncIO_Udp.RequestReliableSendTo(this, ipEndPoint, ref message);
        }

        public bool unreliableSendTo<T>(List<DeviceInfo> ipEndPoints, ref T message, bool isSendMe = false)
        {
            for (int i = 0; i < ipEndPoints.Count; ++i)
            {
                if (AsyncIO_Udp.UnreliableSendTo(this, ipEndPoints[i].waitIPEndPoint, ref message) == false)
                    return false;
            }

            if (isSendMe == true)
                AsyncIO_Udp.SendMe(this, ref message);

            return true;
        }

        public bool reliableSendTo<T>(List<DeviceInfo> ipEndPoints, ref T message, bool isSendMe = false)
        {
            for (int i = 0; i < ipEndPoints.Count; ++i)
            {
                if (AsyncIO_Udp.RequestReliableSendTo(this, ipEndPoints[i].waitIPEndPoint, ref message) == false)
                    return false;
            }

            if (isSendMe == true)
                AsyncIO_Udp.SendMe(this, ref message);

            return true;
        }

        public bool unreliableSendTo<T>(List<EndPoint> ipEndPoints, ref T message, bool isSendMe = false)
        {
            for (int i = 0; i < ipEndPoints.Count; ++i)
            {
                if (AsyncIO_Udp.UnreliableSendTo(this, ipEndPoints[i], ref message) == false)
                    return false;
            }

            if (isSendMe == true)
                AsyncIO_Udp.SendMe(this, ref message);

            return true;
        }

        public bool reliableSendTo<T>(List<EndPoint> ipEndPoints, ref T message, bool isSendMe = false)
        {
            for (int i = 0; i < ipEndPoints.Count; ++i)
            {
                if (AsyncIO_Udp.RequestReliableSendTo(this, ipEndPoints[i], ref message) == false)
                    return false;
            }

            if (isSendMe == true)
                AsyncIO_Udp.SendMe(this, ref message);

            return true;
        }

        internal void tryGetTCBforSACK(EndPoint ipEndPoint, out TCBforSACK outTCBForSACK)
        {
            lock (LockTCB)
            {
                if (TCBrorSACKMap.TryGetValue(ipEndPoint, out outTCBForSACK) == false)
                {
                    outTCBForSACK = new TCBforSACK(UdpMessageBuffer.MaxMessageSize() * 5, 0);
                    TCBrorSACKMap.Add(ipEndPoint, outTCBForSACK);
                }
            }
        }

        public Dictionary<EndPoint, TCBforSACK>.Enumerator getUdpTCBInfoEnumerator()
        {
            return TCBrorSACKMap.GetEnumerator();
        }

        public Dictionary<EndPoint, TCBforSACK> copyTCBforSACK()
        {
            lock (LockTCB)
            {
                return new Dictionary<System.Net.EndPoint, TCBforSACK>(TCBrorSACKMap);
            }
        }

        protected IMessageFactory allocMessageFactory()
        {
            return new UdpMessageFactory();
        }
    }

	public class UdpMessageFactory : MessageFactory<BinaryMessageSerializer, BinaryMessageDeserializer>
	{
	}
}
