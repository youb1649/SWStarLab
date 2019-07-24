using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MNF
{
    public class UdpSessionInfo
    {
        public EndPoint EndPoint { get; set; }
    }

    public class UdpControllHelper : Singleton<UdpControllHelper>
    {
        internal enum ENUM_DROP_TYPE
        {
            DROP_TYPE_SEND,
            DROP_TYPE_RECV,
            DROP_TYPE_SEND_LOOP,
            DROP_TYPE_MAX,
        }

        private Random randomObject = null;
        private int[] dropRateList = null;

        public UdpControllHelper()
        {
            randomObject = new Random();
			dropRateList = new int[(int)ENUM_DROP_TYPE.DROP_TYPE_MAX];
            dropRateList[(int)ENUM_DROP_TYPE.DROP_TYPE_SEND] = 0;
            dropRateList[(int)ENUM_DROP_TYPE.DROP_TYPE_RECV] = 0;
            dropRateList[(int)ENUM_DROP_TYPE.DROP_TYPE_SEND_LOOP] = 1;
        }

        public void SetDropRate(int sendDropRate, int recvDropRate, int ackDropRate)
        {
            dropRateList[(int)ENUM_DROP_TYPE.DROP_TYPE_SEND] = sendDropRate;
            dropRateList[(int)ENUM_DROP_TYPE.DROP_TYPE_RECV] = recvDropRate;
        }

        public void SetSendLoopCount(int sendLoopCount)
        {
            dropRateList[(int)ENUM_DROP_TYPE.DROP_TYPE_SEND_LOOP] = sendLoopCount;
        }

        public int GetSendLoopCount()
        {
            return dropRateList[(int)ENUM_DROP_TYPE.DROP_TYPE_SEND_LOOP];
        }

        internal bool IsDrop(ENUM_DROP_TYPE dropType)
        {
            int randValue = randomObject.Next(1, 101);
            return (randValue < dropRateList[(int)dropType]);
        }
    }

    public class UdpHelper : Singleton<UdpHelper>
    {
        private List<UdpSession> sessions;
        private ThreadAdapter udpSendThreadAdapter = null;
        private DoEventNotifier udpSendThreadEventNotifier = null;
        private object sessionsLock = null;

        public List<UdpSession> Sessions
        {
            get { return sessions; }
        }

        public UdpHelper()
        {
            sessions = new List<UdpSession>();
            var autoResetEvent = new System.Threading.AutoResetEvent(false);
            udpSendThreadAdapter = new ThreadAdapter(autoResetEvent);
            udpSendThreadEventNotifier = new DoEventNotifier(autoResetEvent);
            sessionsLock = new object();
            udpSendThreadAdapter.WaitTime = 100;
        }

        ~UdpHelper()
        {
            Release();
        }

        public bool Run(bool isRunThread)
        {
            if (DispatcherCollection.Instance.Start(
                DISPATCH_TYPE.DISPATCH_UDP, isRunThread) == null)
                return false;

            udpSendThreadAdapter.ThreadEvent += ReliableUdpDatagramSendThread;
            if (udpSendThreadAdapter.Start() == false)
                return false;

            return true;
        }

        public void Release()
		{
			foreach (var udpSession in sessions)
			{
				if (udpSession.Socket == null)
					continue;

				udpSession.Socket.Close();
                udpSession.Socket = null;
			}
		}

        public void DipatchMessage()
        {
            DispatcherCollection.Instance.GetDispatcher(DISPATCH_TYPE.DISPATCH_UDP).dispatchMessage(false);
        }

        public UdpSession Create(Type dispatchExporterType, string bindIPString, string bindPortString)
        {
            try
            {
                var dispatchExporter = DispatchExporterCollection.Instance.Get(dispatchExporterType);
                if (dispatchExporter == null)
                {
                    DispatchExporterCollection.Instance.Add(dispatchExporterType);
                    dispatchExporter = DispatchExporterCollection.Instance.Get(dispatchExporterType);
                }

                if (dispatchExporter.Init() == false)
                    throw new Exception(string.Format("{0} init failed", dispatchExporter.ToString()));

                var udpSession = new UdpSession();
                udpSession.Init();
                udpSession.EndPoint = Utility.GetIPEndPoint(bindIPString, bindPortString);
				udpSession.Socket = AllocUdpSocket(udpSession.EndPoint);
                udpSession.DispatchHelper = dispatchExporter;

                if (AsyncIO_Udp.RecvFrom(udpSession) == false)
                {
                    throw new Exception(string.Format("Udp recvFrom() failed, IP({0}), Port({1})",
                        bindIPString, bindPortString));
                }

                udpSession.IsConnected = true;

                lock (sessionsLock)
                {
                    sessions.Add(udpSession);
                }

                return udpSession;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "create failed, IP({0}), Port({1})", bindIPString, bindPortString);
                return null;
            }
        }

        internal void NotifySendThread()
        {
            udpSendThreadEventNotifier.notify();
        }

        internal Socket AllocUdpSocket(IPEndPoint endPoint)
        {
            var udpSocket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpSocket.ExclusiveAddressUse = false;

            try
            {
                // cut icmp
                var sioUdpConnectionReset = -1744830452;
                var inValue = new byte[] { 0 };
                var outValue = new byte[] { 0 };
                udpSocket.IOControl(sioUdpConnectionReset, inValue, outValue);
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "IOControl() call failed, Maybe your enveriment is OSX");
            }

            udpSocket.Bind(endPoint);

            return udpSocket;
        }

        private void ReliableUdpDatagramSendThread(bool isSignal)
        {
            UdpSession[] sessionList = null;
            lock (sessionsLock)
            {
                if (sessions.Count == 0)
                    return;

                sessionList = new UdpSession[sessions.Count];
                sessions.CopyTo(sessionList);
            }

            // send datagram
            for (int i = 0; i < sessionList.Length; ++i)
                SendReilableDatagram(sessionList[i]);
        }

        private void SendReilableDatagram(UdpSession session)
        {
            var udpTcbInfos = session.copyTCBforSACK();
            var enumerator = udpTcbInfos.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var tcbForSACK = enumerator.Current.Value;
                var nowTicks = DateTime.Now.Ticks;

                if (tcbForSACK.SendableSize == 0)
                    continue;

                AsyncIO_Udp.ReliableSendTo(session, enumerator.Current.Key, tcbForSACK);
                AsyncIO_Udp.ReliableReSendTo(session, enumerator.Current.Key, tcbForSACK, nowTicks);
                AsyncIO_Udp.ReliableSendAckDatagram(session, enumerator.Current.Key, tcbForSACK, nowTicks);
            }
        }
    }
}
