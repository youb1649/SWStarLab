using System;
using System.Text;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

namespace MNF
{
    public class SendMessage
	{
		public int ID { get; private set; }
		public Type Type { get; private set; }
		public object Message { get; private set; }

		public SendMessage(int id, Type type, object message)
		{
			ID = id;
            Type = type;
			Message = message;
		}
	}

	public class RecvMessage
	{
        public bool IsFromServer { get; private set; }
        public bool IsMine { get; private set; }
        public int ID { get; private set; }
		public Type Type { get; private set; }
		public object Message { get; private set; }

		public RecvMessage(bool isFromServer, bool isMine, int id, Type type, object message)
		{
            IsFromServer = isFromServer;
            IsMine = isMine;
            ID = id;
            Type = type;
            Message = message;
		}
	}

    public class DeviceInfo
    {
        public IPEndPoint waitIPEndPoint;
        public IPEndPoint pulseIPEndPoint;
        public bool isServer;
        public string uniqueKey;
        public DateTime registedDateTime;

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}:{3}:{4}", waitIPEndPoint, pulseIPEndPoint, isServer, uniqueKey, registedDateTime);
        }
    }

    class IPPulseLogWriter
    {
        static public void WriteSystem(string formatString, params object[] values)
        {
            LogManager.Instance.WriteSystem("[IPPulse] {0}", string.Format(formatString, values));
        }
        static public void WriteSystemDebug(string formatString, params object[] values)
        {
            LogManager.Instance.WriteSystemDebug("[IPPulse] {0}", string.Format(formatString, values));
        }
        static public void WriteError(string formatString, params object[] values)
        {
            LogManager.Instance.WriteError("[IPPulse] {0}", string.Format(formatString, values));
        }
        static public void WriteException(Exception e, string formatString, params object[] values)
        {
            LogManager.Instance.WriteException(e, "[IPPulse] {0}", string.Format(formatString, values));
        }
    }

    public class IPPulse : Singleton<IPPulse>
    {
        public bool IsRunning { get; private set; }
        public bool IsServer { get; private set; }

        public string MyPort { get; private set; }

        UdpClient MulticastUdpClient { get; set; }
        List<string> MulticastResponseIPList { get; set; }

        UdpClient IPPulseUdpClient { get; set; }
        List<string> embededIPList { get; set; }

        List<DeviceInfo> deviceList = new List<DeviceInfo>();
        long lastCheckTimeoutDevice = 0;
		string GeneratedKey { get; set; }
        string MulticastGeneratedKey { get; set; }
        string IPPulseGeneratedKey { get; set; }
        string ImHereGeneratedKey { get; set; }
        string MessageGeneratedKey { get; set; }

        StringBuilder messageStringBuilder = new StringBuilder(1024 * 100);
		string MulticastCmd { get { return "@MULTICAST@"; } }
		string IPPulseCmd { get { return "@IPPULSE@"; } }
        string IMHERECmd { get { return "@IMHERE@"; } }
        string MessageCmd { get { return "@MESSAGE@"; } }

        IPAddress multicastAddress = IPAddress.Parse("224.0.0.224");
        int multicastPort = 6001;
        int pulsePort = 6000;

        SwapableMessgeQueue<object> sendSwapableMessgeQueue = new SwapableMessgeQueue<object>();
		SwapableMessgeQueue<RecvMessage> recvSwapableMessgeQueue = new SwapableMessgeQueue<RecvMessage>();
		AutoResetEvent sendResetEvent = new AutoResetEvent(false);
		ThreadAdapter SendThreadAdapter;
        AutoResetEvent sendIPPulseResetEvent = new AutoResetEvent(false);
        ThreadAdapter SendIPPulseThreadAdapter;

        bool IsPause { get; set; }

        #region TRACK_SEND_PACKET
        public int RequstSendCount { get; set; }
        public int AsyncSendCount { get; set; }
        public int RecvCount { get; set; }
        #endregion // TRACK_SEND_PACKET

        ~IPPulse()
        {
            Stop();
        }

        public bool Start(List<string> ipList, string port, bool isServer, int sendFrequence = 1000)
        {
            if (IsRunning == true)
            {
                IPPulseLogWriter.WriteSystemDebug("IPPulse has already start");
                return false;
            }

            IsServer = isServer;
            MyPort = port;
            embededIPList = ipList.ToList();
            MulticastResponseIPList = new List<string>();

            GeneratedKey = string.Format("{0}#{1}#{2}", Utility.GetProcessID(), MyPort, IsServer);
            MulticastGeneratedKey = string.Format("{0}#{1}", MulticastCmd, GeneratedKey);
            IPPulseGeneratedKey = string.Format("{0}#{1}", IPPulseCmd, GeneratedKey);
            ImHereGeneratedKey = string.Format("{0}#{1}", IMHERECmd, GeneratedKey);
            MessageGeneratedKey = string.Format("{0}#{1}", MessageCmd, GeneratedKey);

            try
            {
                // create send thread
                SendThreadAdapter = new ThreadAdapter(sendResetEvent);
                SendThreadAdapter.ThreadEvent += SendThread;
                SendThreadAdapter.WaitTime = sendFrequence;
                if (SendThreadAdapter.Start() == false)
                    throw new Exception("Send thread starg failed");

                // create send ippulse thread
                SendIPPulseThreadAdapter = new ThreadAdapter(sendIPPulseResetEvent);
                SendIPPulseThreadAdapter.ThreadEvent += SendIPPulseThread;
                SendIPPulseThreadAdapter.WaitTime = sendFrequence;
                if (SendIPPulseThreadAdapter.Start() == false)
                    throw new Exception("Send IPPulse thread starg failed");

                if (CreateIPPulseUdpClient() == false)
                    throw new Exception("create ippulse udp client failed");

                if (CreateMulticastUdpClient() == false)
                    throw new Exception("create multicast udp client failed");

                IsRunning = true;
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "start() failed");
                Stop();
                return false;
            }

            return true;
        }

        bool CreateIPPulseUdpClient()
        {
            try
            {
                IPPulseUdpClient = new UdpClient();
                IPPulseUdpClient.Client.Bind(Utility.GetIPEndPoint("", pulsePort.ToString()));
                IPPulseUdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), IPPulseUdpClient);
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "start() failed");
                return false;
            }

            return true;
        }

        bool CreateMulticastUdpClient()
        {
            try
            {
                MulticastUdpClient = new UdpClient();
                MulticastUdpClient.Client.Ttl = 10;
                MulticastUdpClient.Client.MulticastLoopback = true;
                MulticastUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                MulticastUdpClient.ExclusiveAddressUse = false;
                MulticastUdpClient.Client.Bind(Utility.GetIPEndPoint("", multicastPort.ToString()));
                MulticastUdpClient.JoinMulticastGroup(multicastAddress);
                MulticastUdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), MulticastUdpClient);
                return true;
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "start() failed");
                return false;
            }
        }

        public void Stop()
        {
            IPPulseLogWriter.WriteSystem("IPPulse.Stop()");
            IsRunning = false;

            try
            {
                if (SendThreadAdapter != null)
                {
				    SendThreadAdapter.Stop();
                    SendThreadAdapter = null;
			    }

                if (SendIPPulseThreadAdapter != null)
                {
                    SendIPPulseThreadAdapter.Stop();
                    SendIPPulseThreadAdapter = null;
                }

                if (IPPulseUdpClient != null)
                {
                    IPPulseUdpClient.Close();
                    IPPulseUdpClient = null;
                }

                if (MulticastUdpClient != null)
                {
                    MulticastUdpClient.Close();
                    MulticastUdpClient = null;
                }
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "Stop() failed");
            }
        }

        public void Pause()
        {
            IPPulseLogWriter.WriteSystem("IPPulse.Pause()");
            IsPause = true;
        }

        public void Resume()
        {
            IPPulseLogWriter.WriteSystem("IPPulse.Resume()");
            IsPause = false;
        }

        public void ClearDeviceList()
        {
            lock (deviceList)
            {
                deviceList.Clear();
            }
        }

        public List<string> GetNetworkIPList()
        {
            lock (embededIPList)
            {
                return embededIPList.ToList();
            }
        }

        string FindSameNetworkArea(string foundIP)
        {
            var copiedIPList = GetNetworkIPList();
            foreach (var ip in copiedIPList)
            {
                byte[] ipBytes = IPAddress.Parse(ip).GetAddressBytes();
                ipBytes[3] = 0;
                var ipAddress = new IPAddress(ipBytes);

                byte[] foundIPBytes = IPAddress.Parse(foundIP).GetAddressBytes();
                foundIPBytes[3] = 0;
                var foundIPAddress = new IPAddress(ipBytes);

                if (ipAddress.ToString() == foundIPAddress.ToString())
                    return ip;
            }

            return "";
        }

        void SendIPPulseThread(bool isSignal)
        {
            // send i'm here.
            if (IsPause == true)
            {
                lock (deviceList)
                {
                    try
                    {
                        foreach (var endPoint in deviceList)
                        {
                            var buffer = Encoding.Unicode.GetBytes(ImHereGeneratedKey);
                            IPPulseUdpClient.Send(buffer, buffer.Length, endPoint.pulseIPEndPoint);
                        }
                    }
                    catch (Exception e)
                    {
                        IPPulseLogWriter.WriteException(e, "SendThread()");
                    }
                }
                Utility.Sleep(200);
                return;
            }

            // send multicast
            try
            {
                var buffer = Encoding.Unicode.GetBytes(MulticastGeneratedKey);
                MulticastUdpClient.Send(buffer, buffer.Length, new IPEndPoint(multicastAddress, multicastPort));
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "SendIPPulseMessage() - MulticastUdpClient");
            }

            // send ippulse
            try
            {
                List<string> copiedIPList = Instance.GetNetworkIPList();
                if (copiedIPList.Count == 0)
                {
                    IPPulseLogWriter.WriteError("SendIPPulseMessage() -> NetworkIPList is empty");
                    return;
                }

                string ipListString = string.Join(", ", copiedIPList);
                foreach (var ip in copiedIPList)
                {
                    if (Instance.MulticastResponseIPList.Contains(ip) == true)
                    {
                        IPPulseLogWriter.WriteSystem("Send skip because multicast address : {0}", ip);
                        continue;
                    }

                    IPPulseLogWriter.WriteSystem("Sending .. {0} -> {1} area[1 ~ 254]", ipListString, ip);

                    var buffer = Encoding.Unicode.GetBytes(IPPulseGeneratedKey);

                    byte[] ipBytes = IPAddress.Parse(ip).GetAddressBytes();
                    byte myLastIP = ipBytes[3];
                    for (byte i = 1; i < 255; ++i)
                    {
                        ipBytes[3] = i;

                        var toIPAddress = new IPAddress(ipBytes);
                        IPPulseUdpClient.Send(buffer, buffer.Length, new IPEndPoint(toIPAddress, pulsePort));

                        Utility.Sleep(20);
                    }
                }
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "SendIPPulseMessage() - IPPulseUdpClient");
            }
        }

        void SendThread(bool isSignal)
        {
            if (sendSwapableMessgeQueue.getReadableQueue().Count == 0)
            {
                lock (sendSwapableMessgeQueue)
                {
                    sendSwapableMessgeQueue.swap();
                }
            }

            if (sendSwapableMessgeQueue.getReadableQueue().Count == 0)
                return;

            var sendMessage = sendSwapableMessgeQueue.getReadableQueue().Peek() as SendMessage;
            sendSwapableMessgeQueue.getReadableQueue().Dequeue();

            var jsonData = JsonSupport.Serialize(sendMessage.Message);
            messageStringBuilder.Length = 0;
            messageStringBuilder.Append(MessageGeneratedKey);
            messageStringBuilder.Append("#");
            messageStringBuilder.Append(sendMessage.ID);
            messageStringBuilder.Append("#");
            messageStringBuilder.Append(sendMessage.Type.FullName);
            messageStringBuilder.Append("#");
            messageStringBuilder.Append(jsonData);

            var sendBytes = Encoding.Unicode.GetBytes(messageStringBuilder.ToString());
            const int MaxPacketSize = 1024 * 60; // max 60 kbytes
            if (sendBytes.Length > MaxPacketSize)
            {
                IPPulseLogWriter.WriteError("udp max packet size{0}:{1}", sendBytes.Length, MaxPacketSize);
                return;
            }

            lock (deviceList)
            {
                try
                {
                    foreach (var endPoint in deviceList)
                    {
                        var sentSize = IPPulseUdpClient.Send(sendBytes, sendBytes.Length, endPoint.pulseIPEndPoint);
                        if (sentSize > 0)
                            ++AsyncSendCount;
                    }
                }
                catch (Exception e)
                {
                    IPPulseLogWriter.WriteException(e, "SendThread()");
                }
            }
        }

        static void ReceiveCallback(IAsyncResult ar)
		{
            var udpClient = (UdpClient)ar.AsyncState;
            if (udpClient == null)
            {
				IPPulseLogWriter.WriteError("udpClient async receive callback is null!");
                return;
            }

            if ((Instance.IPPulseUdpClient == null) || (Instance.MulticastUdpClient == null) || (Instance.IsRunning == false))
                return;

			string recvString = "";
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                Byte[] receiveBytes = udpClient.EndReceive(ar, ref ipEndPoint);
				recvString = Encoding.Unicode.GetString(receiveBytes);
            }
            catch (Exception ex)
            {
				IPPulseLogWriter.WriteException(ex, "Failed to recv data");
            }

            if (recvString.Contains(Instance.MessageCmd) == true)
            {
                Instance.MessageResponse(recvString);
            }
            else if (recvString.Contains(Instance.IPPulseCmd) == true)
			{
                Instance.IPPulseResponse(recvString, ipEndPoint);
			}
			else if (recvString.Contains(Instance.MulticastCmd) == true)
            {
                string foundIP = Instance.FindSameNetworkArea(ipEndPoint.Address.ToString());
                if (foundIP.Length > 0)
                {
				    IPPulseLogWriter.WriteSystem("Add Multicast Response : {0}", ipEndPoint.ToString());
                    Instance.MulticastResponseIPList.Add(ipEndPoint.Address.ToString());
                }

                ipEndPoint.Port = Instance.pulsePort;
                Instance.IPPulseResponse(recvString, ipEndPoint);
            }
			else if (recvString.Contains(Instance.IMHERECmd) == true)
            {
                Instance.ImHereResponse(recvString, ipEndPoint);
            }
            else
            {
				IPPulseLogWriter.WriteError("Not command : {0}", recvString);
			}

            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpClient);
		}

        enum SPLIT_INDEX
        {
            CMD,
            PROCESS_ID,
            TARGET_PORT,
            IS_SERVER,
            MESSAGE_ID,
            MESSAGE_TYPE,
            MESSAGE_JSON,
        }

        public List<RecvMessage> PopReponseMessage()
        {
            if (recvSwapableMessgeQueue.getReadableQueue().Count == 0)
            {
                lock (recvSwapableMessgeQueue)
                {
                    recvSwapableMessgeQueue.swap();
                }
            }

            var recvList = recvSwapableMessgeQueue.getReadableQueue().ToList();
            recvSwapableMessgeQueue.getReadableQueue().Clear();

            RecvCount += recvList.Count;
            return recvList;
        }

        void MessageResponse(string recvString)
        {
            string[] splitedKey = recvString.Split('#');
            if (splitedKey.Length != 7)
                return;

            try
            {
                bool isFromServer = Convert.ToBoolean(splitedKey[(int)SPLIT_INDEX.IS_SERVER]);
                int processID = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.PROCESS_ID]);
                int messageID = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.MESSAGE_ID]);
                string messageType = splitedKey[(int)SPLIT_INDEX.MESSAGE_TYPE];
                string messageJson = splitedKey[(int)SPLIT_INDEX.MESSAGE_JSON];
                bool isMyMessage = (Utility.GetProcessID() == processID);

                Type t = Type.GetType(messageType);
                var message = JsonSupport.DeSerialize(messageJson, t);

                var responseMessage = new RecvMessage(isFromServer, isMyMessage, messageID, t, message);
                lock (recvSwapableMessgeQueue)
                {
                    recvSwapableMessgeQueue.getWritableQueue().Enqueue(responseMessage);
                }
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "MessageResponse() faild");
                return;
            }
        }

        bool SpliteRecvData(string recvString, out string recvUniqueKey, out int recvProcessID, out string recvPort, out bool recvIsServer)
        {
            recvUniqueKey = "";
            recvProcessID = 0;
            recvPort = "";
            recvIsServer = false;

            string[] splitedKey = recvString.Split('#');
            if (splitedKey.Length != 4)
                return false;

            try
            {
                recvUniqueKey = splitedKey[(int)SPLIT_INDEX.PROCESS_ID];
                recvProcessID = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.PROCESS_ID]);
                recvPort = splitedKey[(int)SPLIT_INDEX.TARGET_PORT];
                recvIsServer = Convert.ToBoolean(splitedKey[(int)SPLIT_INDEX.IS_SERVER]);
            }
            catch (Exception e)
            {
                IPPulseLogWriter.WriteException(e, "SpliteRecvData() split failed");
                return false;
            }

            return true;
        }

        void UpdateDevice(IPEndPoint remoteIP, string recvUniqueKey, string recvPort, bool recvIsServer)
        {
            lock (deviceList)
            {
                var recvIPEndPoint = Utility.GetIPEndPoint(remoteIP.Address.ToString(), recvPort);
                try
                {
                    foreach (var endPoint in deviceList)
                    {
                        if ((endPoint.waitIPEndPoint.ToString() == recvIPEndPoint.ToString()) && (recvUniqueKey == endPoint.uniqueKey))
                        {
                            endPoint.registedDateTime = DateTime.Now;
                            return;
                        }
                    }

                    var responseEndPointInfo = new DeviceInfo()
                    {
                        waitIPEndPoint = recvIPEndPoint,
                        pulseIPEndPoint = remoteIP,
                        isServer = recvIsServer,
                        uniqueKey = recvUniqueKey,
                        registedDateTime = DateTime.Now
                    };
                    deviceList.Add(responseEndPointInfo);
                    
                    IPPulseLogWriter.WriteSystem("Add : {0}", responseEndPointInfo.ToString());
                }
                catch (Exception e)
                {
                    IPPulseLogWriter.WriteException(e, "UpdateDevice() device list update failed");
                }
            }
        }

        void IPPulseResponse(string recvString, IPEndPoint remoteIP)
        {
            string recvUniqueKey = "";
            int recvProcessID = 0;
            string recvPort = "";
			bool recvIsServer = false;
            if (SpliteRecvData(recvString, out recvUniqueKey, out recvProcessID, out recvPort, out recvIsServer) == false)
            {
                IPPulseLogWriter.WriteError("IPPulseResponse() recvString({0}) split failed", recvString);
                return;
            }

            if (Utility.GetProcessID() == recvProcessID)
                return;

            // send i'm here.
            var buffer = Encoding.Unicode.GetBytes(ImHereGeneratedKey);
            IPPulseUdpClient.Send(buffer, buffer.Length, remoteIP);
            IPPulseLogWriter.WriteSystemDebug("Send I'm here - To:{0}", remoteIP);

            UpdateDevice(remoteIP, recvUniqueKey, recvPort, recvIsServer);
        }

        void ImHereResponse(string recvString, IPEndPoint remoteIP)
        {
            string recvUniqueKey = "";
            int recvProcessID = 0;
            string recvPort = "";
            bool recvIsServer = false;
            if (SpliteRecvData(recvString, out recvUniqueKey, out recvProcessID, out recvPort, out recvIsServer) == false)
            {
                IPPulseLogWriter.WriteError("IPPulseResponse() recvString({0}) split failed", recvString);
                return;
            }

            UpdateDevice(remoteIP, recvUniqueKey, recvPort, recvIsServer);
        }

        public void SendMessage<T>(int messageID, T managedData) where T : new()
        {
            var sendMessage = new SendMessage(messageID, typeof(T), managedData);
			lock (sendSwapableMessgeQueue)
			{
                sendSwapableMessgeQueue.getWritableQueue().Enqueue(sendMessage);
			}
            sendResetEvent.Set();

            ++RequstSendCount;
        }

        public void RemoveTimeoutDevice(int timeoutSecond)
        {
            long nextCheckTime = (Utility.ConvertToUnixTime(DateTime.Now) + timeoutSecond);
            if (lastCheckTimeoutDevice >= nextCheckTime)
                return;

            lastCheckTimeoutDevice = nextCheckTime;

            lock (deviceList)
            {
                for (int i = 0; i < deviceList.Count; ++i)
                {
                    if (deviceList[i].registedDateTime.AddSeconds(timeoutSecond) < DateTime.Now)
                    {
                        IPPulseLogWriter.WriteSystemDebug("Remove : {0}", deviceList[i].ToString());
                        deviceList.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        public List<DeviceInfo> GetDeviceList(bool onlyServer = false)
        {
            lock (deviceList)
            {
                var returnDeviceList = new List<DeviceInfo>(deviceList.Count);

                for (int i = 0; i < deviceList.Count; ++i)
                {
                    if (onlyServer == true)
                    {
                        if (deviceList[i].isServer == true)
                            returnDeviceList.Add(deviceList[i]);
                    }
                    else
                    {
                        returnDeviceList.Add(deviceList[i]);
                    }
                }

                return returnDeviceList;
            }
        }

        public bool FindDevice(int findPort, bool onlyServer, out string outIP)
        {
            outIP = "";

            var deviceList = IPPulse.Instance.GetDeviceList(onlyServer);
            foreach (var deviceInfo in deviceList)
            {
                if (deviceInfo.waitIPEndPoint.Port == findPort)
                {
                    outIP = deviceInfo.waitIPEndPoint.Address.ToString();
                    return true;
                }
            }

            return false;
        }
    }
}
