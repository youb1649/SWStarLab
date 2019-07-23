using System;
using System.Text;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

//namespace MNF
//{
//	public class LookAroundUdpClient : UdpClient
//    {
//        public IPEndPoint MyBindEndPoint { get; set; }
//        public IPEndPoint MyMulticastIPEndPoint { get; set; }
//    }

//    public class MyIPInfo
//    {
//        public int responseCount;
//        public string ip;
//    }

//    public class LookAround : Singleton<LookAround>
//    {
//        List<LookAroundUdpClient> udpClients = new List<LookAroundUdpClient>();
//        List<MyIPInfo> myIPs = new List<MyIPInfo>();

//        public bool IsRunning { get; private set; }

//        public bool IsServer { get; private set; }
//        public bool IsFoundServer { get; private set; }

//        public string ServerIP { get; private set; }
//        public string ServerPort { get; private set; }

//        public bool IsSetMyInfo { get; private set; }
//        public string MyPort { get; private set; }
        
//        IPAddress multicastaddress = IPAddress.Parse("224.0.0.224");
//		int multicastPort = 60000;

//        List<DeviceInfo> responseEndPoint = new List<DeviceInfo>();
//        long lastCheckTimeoutDevice = 0;
//		string GeneratedKey { get; set; }
//        string LookAroundGeneratedKey { get; set; }
//        string MessageGeneratedKey { get; set; }

//		StringBuilder messageStringBuilder = new StringBuilder(1024 * 100);
//		string LookAroundCmd { get { return "@LOOKAROUND@"; } }
//        string MessageCmd { get { return "@MESSAGE@"; } }

//		SwapableMessgeQueue<object> sendSwapableMessgeQueue = new SwapableMessgeQueue<object>();
//		SwapableMessgeQueue<RecvMessage> recvSwapableMessgeQueue = new SwapableMessgeQueue<RecvMessage>();
//		AutoResetEvent sendResetEvent = new AutoResetEvent(false);
//		ThreadAdapter lookSendThreadAdapter;

//        #region TRACK_SEND_PACKET
//        public int RequstSendCount { get; set; }
//        public int AsyncSendCount { get; set; }
//        public int RecvCount { get; set; }
//        #endregion // TRACK_SEND_PACKET

//        ~LookAround()
//        {
//            Stop();
//        }

//        public void RemoveTimeoutDevice(int timeoutSecond)
//        {
//            long nextCheckTime = (Utility.ConvertToUnixTime(DateTime.Now) + timeoutSecond);
//            if (lastCheckTimeoutDevice >= nextCheckTime)
//                return;

//            lastCheckTimeoutDevice = nextCheckTime;

//            lock (responseEndPoint)
//            {
//                for (int i = 0; i < responseEndPoint.Count; ++i)
//                {
//                    if (responseEndPoint[i].registedDateTime.AddSeconds(timeoutSecond) < DateTime.Now)
//                    {
//                        responseEndPoint.RemoveAt(i);
//                        --i;
//                    }
//                }
//            }
//        }

//        public string GetMyIP()
//        {
//            lock (myIPs)
//            {
//                if (myIPs.Count == 0)
//                    return "";

//                if (myIPs.Count == 1)
//                {
//                    return myIPs[0].ip;
//                }

//                int maxScoreIndex = 0;
//                string maxScoreIP = "";
//                foreach (var myip in myIPs)
//                {
//                    if (myip.responseCount > maxScoreIndex)
//                    {
//                        maxScoreIndex = myip.responseCount;
//                        maxScoreIP = myip.ip;
//                    }
//                }
//                return maxScoreIP;
//            }
//        }

//        public List<DeviceInfo> GetResponseEndPoint()
//        {
//            lock (responseEndPoint)
//            {
//				var returnEndPoints = new List<DeviceInfo>(responseEndPoint.Count);

//				for (int i = 0; i < responseEndPoint.Count; ++i)
//                    returnEndPoints.Add(responseEndPoint[i]);

//                return returnEndPoints;
//            }
//        }

//		public void SetSendFrequence(int sendFrequence)
//		{
//            if (lookSendThreadAdapter != null)
//    			lookSendThreadAdapter.WaitTime = sendFrequence;
//		}

//        public bool Start(List<string> ipList, string port, bool isServer, int sendFrequence = 1000)
//        {
//            if (IsRunning == true)
//            {
//                LogManager.Instance.WriteSystem("LookAround has already start");
//                return false;
//            }

//            IsServer = isServer;
//            MyPort = port;

//            GeneratedKey = string.Format("{0}#{1}#{2}", Utility.GetProcessID(), MyPort, IsServer);
//            LookAroundGeneratedKey = string.Format("{0}#{1}", LookAroundCmd, GeneratedKey);
//            MessageGeneratedKey = string.Format("{0}#{1}", MessageCmd, GeneratedKey);

//            try
//            {
//                // send thread
//                lookSendThreadAdapter = new ThreadAdapter(sendResetEvent);
//                lookSendThreadAdapter.ThreadEvent += LookSendThread;
//                lookSendThreadAdapter.WaitTime = sendFrequence;
//                if (lookSendThreadAdapter.Start() == false)
//                    throw new Exception("Send thread starg failed");

//                foreach (var ip in ipList)
//                {
//                    byte[] ipBytes = IPAddress.Parse(ip).GetAddressBytes();
//                    byte[] mulitcastBytes = multicastaddress.GetAddressBytes();

//                    for (byte i = 0; i < 1; ++i)
//                    {
//                        mulitcastBytes[3] = ipBytes[0];
//                        mulitcastBytes[3] -= i;
//                        AddUdpClient(ip, new IPAddress(mulitcastBytes), (multicastPort - mulitcastBytes[3]), sendFrequence);
//                    }
//                }
//                AddUdpClient("", multicastaddress, multicastPort, sendFrequence);

//                IsRunning = true;
//            }
//            catch (Exception e)
//            {
//                LogManager.Instance.WriteException(e, "start() failed");
//                Stop();
//                return false;
//            }

//            return true;
//        }

//        private void AddUdpClient(string myIP, IPAddress myMulticastAddress, int myMulticastPort, int sendFrequence)
//        {
//            var udpClient = new LookAroundUdpClient();
//            udpClient.Client.Ttl = 10;
//            udpClient.Client.MulticastLoopback = true;
//            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
//            udpClient.ExclusiveAddressUse = false;
//            udpClient.MyMulticastIPEndPoint = new IPEndPoint(myMulticastAddress, myMulticastPort);
//            udpClient.MyBindEndPoint = Utility.GetIPEndPoint(myIP, myMulticastPort.ToString());
//            //udpClient.MyBindEndPoint = new IPEndPoint(IPAddress.Any, myMulticastPort);

//            LogManager.Instance.Write("Client Bind:{0} Multicast:{1}", udpClient.MyBindEndPoint, udpClient.MyMulticastIPEndPoint);
//            udpClient.Client.Bind(udpClient.MyBindEndPoint);
//            udpClient.JoinMulticastGroup(myMulticastAddress);

//            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpClient);
//            udpClients.Add(udpClient);
//        }

//        public void Stop()
//        {
//            LogManager.Instance.Write("LookAround.Stop()");
//            IsRunning = false;

//            if (udpClients != null)
//            {
//                foreach (var udpClient in udpClients)
//                {
//                    udpClient.Close();
//                }
//                udpClients = null;
//            }

//            if (lookSendThreadAdapter != null)
//            {
//				lookSendThreadAdapter.Stop();
//                lookSendThreadAdapter = null;
//			}
//        }

//        void SendLookAroundMessage()
//        {
//            try
//            {
//                foreach (var udpClient in udpClients)
//                {
//                    var buffer = Encoding.Unicode.GetBytes(LookAroundGeneratedKey);
//                    LogManager.Instance.Write("SendLookAroundMessage - Bind:{0} Multicast:{1}",
//                        udpClient.MyBindEndPoint, udpClient.MyMulticastIPEndPoint);
//                    udpClient.Send(buffer, buffer.Length, udpClient.MyMulticastIPEndPoint);
//                }
//            }
//            catch (Exception e)
//            {
//                LogManager.Instance.WriteException(e, "lookSendThread");
//            }
//        }

//        void LookSendThread(bool isSignal)
//        {
//            if (isSignal == false)
//            {
//                SendLookAroundMessage();
//            }

//            if (sendSwapableMessgeQueue.getReadableQueue().Count == 0)
//			{
//				lock (sendSwapableMessgeQueue)
//				{
//					sendSwapableMessgeQueue.swap();
//				}
//			}

//            if (sendSwapableMessgeQueue.getReadableQueue().Count == 0)
//                return;

//            var sendMessage = sendSwapableMessgeQueue.getReadableQueue().Peek() as SendMessage;
//			sendSwapableMessgeQueue.getReadableQueue().Dequeue();

//            var jsonData = JsonSupport.Serialize(sendMessage.Message);
//			messageStringBuilder.Length = 0;
//			messageStringBuilder.Append(MessageGeneratedKey);
//			messageStringBuilder.Append("#");
//            messageStringBuilder.Append(sendMessage.ID);
//			messageStringBuilder.Append("#");
//            messageStringBuilder.Append(sendMessage.Type.FullName);
//			messageStringBuilder.Append("#");
//			messageStringBuilder.Append(jsonData);

//			var sendBytes = Encoding.Unicode.GetBytes(messageStringBuilder.ToString());
//            const int MaxPacketSize = 1024 * 60; // max 60 kbytes
//            if (sendBytes.Length > MaxPacketSize)
//				LogManager.Instance.WriteError("udp max packet size{0}:{1}", sendBytes.Length, MaxPacketSize);

//            foreach(var udpClient in udpClients)
//            {
//                LogManager.Instance.Write("Send From:{0} To:{1}", udpClient.MyBindEndPoint, udpClient.MyMulticastIPEndPoint);
//                var sentSize = udpClient.Send(sendBytes, sendBytes.Length, udpClient.MyMulticastIPEndPoint);
//                if (sentSize > 0)
//                    ++AsyncSendCount;
//            }
//        }

//        static void ReceiveCallback(IAsyncResult ar)
//		{
//            var udpClient = (UdpClient)ar.AsyncState;
//            if (udpClient == null)
//            {
//				LogManager.Instance.WriteError("udpClient async receive callback is null!");
//                return;
//            }

//			string recvString = "";
//            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
//            try
//            {
//                Byte[] receiveBytes = udpClient.EndReceive(ar, ref ipEndPoint);
//				recvString = Encoding.Unicode.GetString(receiveBytes);
//            }
//            catch (Exception ex)
//            {
//				LogManager.Instance.WriteException(ex, "Failed to recv data");
//            }

//            LogManager.Instance.WriteSystem("Recevied from {0} Length:{1}", ipEndPoint, recvString.Length);

//            if (recvString.Contains(Instance.MessageCmd) == true)
//			{
//                LookAround.Instance.MessageResponse(recvString);
//			}
//            else if (recvString.Contains(Instance.LookAroundCmd) == true)
//			{
//                LookAround.Instance.LookAroundResponse(recvString, ipEndPoint.Address.ToString());
//			}
//			else
//			{
//				LogManager.Instance.WriteError("Not command : {0}", recvString);
//			}

//            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpClient);
//		}

//		enum SPLIT_INDEX
//		{
//            CMD,
//            PROCESS_ID,
//            TARGET_PORT,
//            IS_SERVER,
//            MESSAGE_ID,
//            MESSAGE_TYPE,
//            MESSAGE_JSON,
//		}
//        public List<RecvMessage> PopReponseMessage()
//        {
//            if (recvSwapableMessgeQueue.getReadableQueue().Count == 0)
//            {
//                lock (recvSwapableMessgeQueue)
//                {
//					recvSwapableMessgeQueue.swap();
//				}
//            }

//            var recvList = recvSwapableMessgeQueue.getReadableQueue().ToList();
//            recvSwapableMessgeQueue.getReadableQueue().Clear();

//            RecvCount += recvList.Count;
//            return recvList;
//        }

//        void MessageResponse(string recvString)
//        {
//            string[] splitedKey = recvString.Split('#');
//            if (splitedKey.Length != 7)
//                return;

//			try
//			{
//				bool isFromServer = Convert.ToBoolean(splitedKey[(int)SPLIT_INDEX.IS_SERVER]);
//				int processID = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.PROCESS_ID]);
//                int messageID = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.MESSAGE_ID]);
//				string messageType = splitedKey[(int)SPLIT_INDEX.MESSAGE_TYPE];
//                string messageJson = splitedKey[(int)SPLIT_INDEX.MESSAGE_JSON];
//                bool isMyMessage = (Utility.GetProcessID() == processID);

//                Type t = Type.GetType(messageType);
//				var message = JsonSupport.DeSerialize(messageJson, t);

//                var responseMessage = new RecvMessage(isFromServer, isMyMessage, messageID, t, message);
//                lock (recvSwapableMessgeQueue)
//                {
//					recvSwapableMessgeQueue.getWritableQueue().Enqueue(responseMessage);
//				}
//			}
//			catch (Exception e)
//			{
//				LogManager.Instance.WriteException(e, "2. lookRecvThread");
//				return;
//			}
//        }

//        void LookAroundResponse(string recvString, string remoteIP)
//        {
//			string[] splitedKey = recvString.Split('#');
//			if (splitedKey.Length != 4)
//				return;

//			string uniqueKey = splitedKey[(int)SPLIT_INDEX.PROCESS_ID];
//			string port = "";
//			bool isServer = false;
//			try
//			{
//                int processID = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.PROCESS_ID]);
//                port = splitedKey[(int)SPLIT_INDEX.TARGET_PORT];
//                isServer = Convert.ToBoolean(splitedKey[(int)SPLIT_INDEX.IS_SERVER]);

//				if (isServer == true)
//				{
//					IsFoundServer = true;
//					ServerIP = remoteIP;
//					ServerPort = port;
//				}

//                if (Utility.GetProcessID() == processID)
//				{
//                    lock(myIPs)
//                    {
//                        MyIPInfo myIPInfo = null;
//                        for (int i = 0; i < myIPs.Count; ++i)
//                        {
//                            if (myIPs[i].ip == remoteIP)
//                            {
//                                myIPs[i].responseCount += 1;
//                                myIPInfo = myIPs[i];
//                                break;
//                            }
//                        }

//                        if (myIPInfo == null)
//                        {
//                            myIPInfo = new MyIPInfo()
//                            {
//                                ip = remoteIP,
//                                responseCount = 1
//                            };
//                            myIPs.Add(myIPInfo);
//                        }

//                        IsSetMyInfo = true;

//                        foreach (var ip in myIPs)
//                        {
//                            LogManager.Instance.WriteSystem("My IP : {0}, MyPort : {1}", myIPInfo.ip, port);
//                        }
//                    }
//                    return;
//				}
//			}
//			catch (Exception e)
//			{
//				LogManager.Instance.WriteException(e, "2. lookRecvThread");
//				return;
//			}

//			lock (responseEndPoint)
//			{
//				var recvIPEndPoint = Utility.GetIPEndPoint(remoteIP, port);
//				try
//				{
//					foreach (var endPoint in responseEndPoint)
//					{
//                        if ((endPoint.waitIPEndPoint.ToString() == recvIPEndPoint.ToString()) && (uniqueKey == endPoint.uniqueKey))
//						{
//                            endPoint.registedDateTime = DateTime.Now;
//							recvIPEndPoint = null;
//							break;
//						}
//					}
//				}
//				catch (Exception e)
//				{
//					LogManager.Instance.WriteException(e, "3. lookRecvThread");
//					return;
//				}

//				try
//				{
//					if (recvIPEndPoint == null)
//						return;

//                    var responseEndPointInfo = new DeviceInfo()
//                    {
//                        waitIPEndPoint = recvIPEndPoint,
//                        isServer = isServer,
//                        uniqueKey = uniqueKey,
//                        registedDateTime = DateTime.Now
//                    };
//                    responseEndPoint.Add(responseEndPointInfo);

//					LogManager.Instance.WriteSystem("Add : {0}", recvIPEndPoint.ToString());
//				}
//				catch (Exception e)
//				{
//					LogManager.Instance.WriteException(e, "4. lookRecvThread");
//					return;
//				}
//			}
//        }

//        public void SendMessage<T>(int messageID, T managedData) where T : new()
//        {
//            var sendMessage = new SendMessage(messageID, typeof(T), managedData);
//			lock (sendSwapableMessgeQueue)
//			{
//                sendSwapableMessgeQueue.getWritableQueue().Enqueue(sendMessage);
//			}
//            sendResetEvent.Set();

//            ++RequstSendCount;
//        }
//    }
//}
