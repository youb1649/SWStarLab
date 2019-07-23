using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using MNF;

public static class P2P_Message
{
    public enum SystemMessageType
    {
        CONNECT = -100,
        CONNECT_NOTIFY,
        CONNECT_INFO,
        DISCONNECT,
    }

    [Serializable]
    public class CONNECT_NOTIFY
    {
        public ConnectionInfo connectionInfo = new ConnectionInfo();
    }

    [Serializable]
    public class CONNECT_INFO
    {
        public List<ConnectionInfo> ConnectionList = new List<ConnectionInfo>();
    }
}

[Serializable]
public class SystemMessage
{
    public int HostId;
    public int ConnectId;
    public int ChannelId;

    public IPEndPoint EndPoint;
    public NetworkID Network;
    public NodeID DstNode;

    public NetworkError NetworkError;
}

[Serializable]
public struct ConnectionInfo
{
    public int HostId;
    public int ConnectionId;
    public string IP;
    public string Port;
    public UInt64 NetworkID;
    public ushort DstNode;
}

public class HostInfo
{
    public int HostId { get; set; }
    public string IP { get; set; }
    public int Port { get; set; }
    public ConnectionConfig ConnectionConfig { get; set; }
    public Dictionary<int, ConnectionInfo> ConnectionList { get; set; }
    public Dictionary<QosType, int> Channels { get; set; }

    public HostInfo()
    {
        ConnectionList = new Dictionary<int, ConnectionInfo>();
        Channels = new Dictionary<QosType, int>();
    }
}

public class UNetUtil
{
    /*
    public enum NetworkError
    {
        Ok,
        WrongHost,
        WrongConnection,
        WrongChannel,
        NoResources,
        BadMessage,
        Timeout,
        MessageToLong,
        WrongOperation,
        VersionMismatch,
        CRCMismatch,
        DNSFailure,
        UsageError
    }
    */
    public static bool LogNetworkError(byte error)
    {
        if (error == (byte)NetworkError.Ok)
        {
            return true;
        }
        else
        {
            NetworkError nerror = (NetworkError)error;
            LogManager.Instance.WriteError("Error : {0}", nerror);
            return false;
        }
    }
}

public class P2PController : MonoBehaviour
{
    enum SPLIT_INDEX
    {
        MESSAGE_ID,
        TYPE_NAME,
        MESSAGE_JSON,
    }

    StringBuilder messageStringBuilder = new StringBuilder(1024 * 100);
    const int bufferSize = 10240;
    byte[] recvBuffer = new byte[bufferSize];

    public HostInfo Host { get; private set; }
    public int MyConnectionId { get { return 0; } }
    public int DefaultMaxConnect { get { return 10; } }

    public bool IsInit { get; private set; }
    public bool IsServer { get; private set; }

    public bool Init(string port, bool isServer, QosType[] qosTypes, int maxDefaultConnection)
    {
        if (IsInit == true)
            return true;

        GlobalConfig gc = new GlobalConfig();
        gc.ReactorModel = ReactorModel.FixRateReactor;
        gc.ThreadAwakeTimeout = 10;
        NetworkTransport.Init(gc);

        Host = new HostInfo();
        Host.IP = IPAddress.Any.ToString();
        Host.Port = Convert.ToInt32(port);
        Host.ConnectionConfig = new ConnectionConfig();
        foreach (QosType qosType in qosTypes)
        {
            Host.Channels.Add(qosType, Host.ConnectionConfig.AddChannel(qosType));
        }
        Host.HostId = NetworkTransport.AddHost(new HostTopology(Host.ConnectionConfig, maxDefaultConnection), Host.Port);

        IsInit = true;
        IsServer = isServer;

        return true;
    }

    public bool AddHost(QosType[] qosTypes, int maxDefaultConnection, int hostPort)
    {
        var hostInfo = new HostInfo();
        hostInfo.ConnectionConfig = new ConnectionConfig();
        foreach (QosType qosType in qosTypes)
        {
            hostInfo.Channels.Add(qosType, hostInfo.ConnectionConfig.AddChannel(qosType));
        }
        hostInfo.HostId = NetworkTransport.AddHost(new HostTopology(hostInfo.ConnectionConfig, maxDefaultConnection), hostPort);
        hostInfo.Port = hostPort;

        return true;
    }

    public Dictionary<int, ConnectionInfo>.Enumerator GetConnectList()
    {
        return Host.ConnectionList.GetEnumerator();
    }

    public void Release()
    {
        if (Host == null)
            return;

        foreach (var connectionInfo in Host.ConnectionList)
        {
            Disconnect(connectionInfo.Value.ConnectionId);
        }

        IsInit = false;
    }

    public bool StartServer(string port)
    {
        QosType[] qosType = { QosType.Reliable, QosType.Unreliable };
        if (Init(port, true, qosType, DefaultMaxConnect) == false)
        {
            LogManager.Instance.WriteError("P2PController Init({0}) failed", port);
            return false;
        }

        LogManager.Instance.WriteSystem("P2PController Init({0}) succeed", port);
        return true;
    }

    public bool StartClient(string serverIP, string port)
    {
        QosType[] qosType = { QosType.Reliable, QosType.Unreliable };
        if (Init(port, false, qosType, DefaultMaxConnect) == false)
        {
            LogManager.Instance.WriteError("P2PController Init({0}) failed", port);
            return false;
        }

        if (Connect(serverIP, port) == true)
        {
            LogManager.Instance.WriteSystem("P2PController Connect({0}:{1}) succeed", serverIP, port);
        }
        else
        {
            LogManager.Instance.WriteError("P2PController Connect({0}:{1}) failed", serverIP, port);
            return false;
        }

        return true;
    }

    public bool Connect(string ip, string port)
    {
        byte error = 0;
        var connectionId = NetworkTransport.Connect(Host.HostId, ip, Convert.ToInt32(port), 0, out error);
        LogManager.Instance.WriteSystem("hostId:{0} Connect:{1} to {2}:{3}", Host.HostId, connectionId, ip, port, error);
        return (error == (byte)NetworkError.Ok);
    }

    public bool IsConnected(int connectionId)
    {
        return Host.ConnectionList.ContainsKey(connectionId);
    }

    public void Disconnect(int connectionId)
    {
        byte error = 0;
        NetworkTransport.Disconnect(Host.HostId, connectionId, out error);
        LogManager.Instance.WriteSystem("hostId:{0} Disconnect:{1} error:{2}", Host.HostId, connectionId, error);
    }

    public bool SendMessage(int connectionId, QosType channelId, int messageId, object messageData)
    {
        /*
         * Channel {0} for connection {1} does not support fragmented messages; 
         * MTU: {1440}, message length: {3176}, max length {1402}
         */
        var serializedMessage = Serialize(messageId, messageData);
        if (serializedMessage == null)
            return false;

        byte error = 0;
        NetworkTransport.Send(Host.HostId, connectionId, Host.Channels[channelId], serializedMessage, serializedMessage.Length, out error);
        return UNetUtil.LogNetworkError(error);
    }

    public bool Broadcast(QosType channelId, int messageId, object messageData, bool loopback = true)
    {
        var serializedMessage = Serialize(messageId, messageData);
        if (serializedMessage == null)
            return false;

        foreach (var connectionInfo in Host.ConnectionList)
        {
            byte error = 0;
            NetworkTransport.Send(Host.HostId, connectionInfo.Value.ConnectionId, Host.Channels[channelId], serializedMessage, serializedMessage.Length, out error);
            UNetUtil.LogNetworkError(error);
        }

        if (loopback == true)
            MessageDispatcher(Host.HostId, MyConnectionId, Host.Channels[channelId], messageId, messageData);

        return true;
    }

    byte[] Serialize<T>(int messageId, T messageData) where T : new()
    {
        try
        {
            var jsonMessage = JsonSupport.Serialize(messageData);
            messageStringBuilder.Length = 0;
            messageStringBuilder.Append(messageId);
            messageStringBuilder.Append('#');
            messageStringBuilder.Append(messageData.GetType().FullName);
            messageStringBuilder.Append('#');
            messageStringBuilder.Append(jsonMessage);

            // ... compress lz4
            //var sendBytes = Encoding.Unicode.GetBytes(jsonMessage);

            return Encoding.Unicode.GetBytes(messageStringBuilder.ToString());
        }
        catch (Exception e)
        {
            LogManager.Instance.WriteException(e, "messageId:{0} serialize failed", messageId);
            return null;
        }
    }

    bool ParseMessage(string recvString, out int messageId, out object messageData)
    {
        string[] splitedKey = recvString.Split('#');
        if (splitedKey.Length != 3)
        {
            messageId = 0;
            messageData = null;
            return false;
        }

        try
        {
            messageId = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.MESSAGE_ID]);
            Type type = Type.GetType(splitedKey[(int)SPLIT_INDEX.TYPE_NAME]);
            messageData = JsonSupport.DeSerialize(splitedKey[(int)SPLIT_INDEX.MESSAGE_JSON], type);
            return true;
        }
        catch (Exception e)
        {
            LogManager.Instance.WriteException(e, "ParseMessage Failed:{0}", recvString);
            messageId = 0;
            messageData = null;
            return false;
        }
    }

    protected virtual void Update()
    {
        if (IsInit == true)
            ReceiveMessage();
    }

    void ReceiveMessage()
    {
        int recHostId = 0;
        int connectionId = 0;
        int channelId = 0;
        int dataSize = 0;
        byte error = 0;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recvBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing:
                return;
            case NetworkEventType.ConnectEvent:
                {
                    LogManager.Instance.WriteSystem("ConnectEvent : recData:{0} recHostId:{1} connectionId:{2} channelId:{3} dataSize:{4} error:{5}",
                                            recData, recHostId, connectionId, channelId, dataSize, (NetworkError)error);

                    string address;
                    int port;
                    NetworkID network;
                    NodeID dstNode;
                    NetworkTransport.GetConnectionInfo(recHostId, connectionId, out address, out port, out network, out dstNode, out error);

                    LogManager.Instance.Write("address:{0} port:{1} network:{2} dstNode:{3} error:{4}",
                                            address, port, network, dstNode, (NetworkError)error);

                    var connectMessage = new SystemMessage();
                    connectMessage.HostId = recHostId;
                    connectMessage.ConnectId = connectionId;
                    connectMessage.ChannelId = channelId;
                    connectMessage.EndPoint = MNF.Utility.GetIPEndPoint(address, port.ToString());
                    connectMessage.Network = network;
                    connectMessage.DstNode = dstNode;
                    connectMessage.NetworkError = (NetworkError)error;
                    ProcRecviedMessage(recHostId, connectionId, channelId, (int)P2P_Message.SystemMessageType.CONNECT, connectMessage);
                }
                break;
            case NetworkEventType.DataEvent:
                {
                    LogManager.Instance.WriteDebug("DataEvent : recData:{0} recHostId:{1} connectionId:{2} channelId:{3} dataSize:{4} error:{5}",
                                        recData, recHostId, connectionId, channelId, dataSize, (NetworkError)error);

                    string recvString = Encoding.Unicode.GetString(recvBuffer, 0, dataSize);
                    int messageId = 0;
                    object messageData;
                    if (ParseMessage(recvString, out messageId, out messageData) == true)
                    {
                        ProcRecviedMessage(recHostId, connectionId, channelId, messageId, messageData);
                    }
                }
                break;
            case NetworkEventType.DisconnectEvent:
                {
                    LogManager.Instance.WriteSystem("DisconnectEvent : recData:{0} recHostId:{1} connectionId:{2} channelId:{3} dataSize:{4} error:{5}",
                                        recData, recHostId, connectionId, channelId, dataSize, (NetworkError)error);

                    var disconnectMessage = new SystemMessage();
                    disconnectMessage.HostId = recHostId;
                    disconnectMessage.ConnectId = connectionId;
                    disconnectMessage.ChannelId = channelId;
                    disconnectMessage.NetworkError = (NetworkError)error;
                    ProcRecviedMessage(recHostId, connectionId, channelId, (int)P2P_Message.SystemMessageType.DISCONNECT, disconnectMessage);
                }
                break;
        }
    }

    void ProcRecviedMessage(int hostId, int connectionId, int channelId, int messageId, object messageData)
    {
        switch (messageId)
        {
            case (int)P2P_Message.SystemMessageType.CONNECT:
                {
                    var connectMessage = (SystemMessage)messageData;
                    ConnectionInfo connectionInfo = new ConnectionInfo();
                    connectionInfo.HostId = hostId;
                    connectionInfo.ConnectionId = connectionId;
                    connectionInfo.IP = connectMessage.EndPoint.Address.ToString();
                    connectionInfo.Port = connectMessage.EndPoint.Port.ToString();

                    if (IsServer == true)
                    {
                        var connectNotify = new P2P_Message.CONNECT_NOTIFY();
                        connectNotify.connectionInfo = connectionInfo;
                        Broadcast(QosType.Reliable, (int)P2P_Message.SystemMessageType.CONNECT_NOTIFY, connectNotify, false);

                        if (Host.ConnectionList.Count > 0)
                        {
                            var memberInfo = new P2P_Message.CONNECT_INFO();
                            foreach (var connectInfo in Host.ConnectionList)
                            {
                                memberInfo.ConnectionList.Add(connectInfo.Value);
                            }
                            SendMessage(connectionId, QosType.Reliable, (int)P2P_Message.SystemMessageType.CONNECT_INFO, memberInfo);
                        }
                    }
                    Host.ConnectionList.Add(connectionId, connectionInfo);
                }
                break;

            case (int)P2P_Message.SystemMessageType.CONNECT_INFO:
                {
                    if (IsServer == false)
                    {
                        var memberInfo = (P2P_Message.CONNECT_INFO)messageData;
                        foreach (var connectionInfo in memberInfo.ConnectionList)
                        {
                            Connect(connectionInfo.IP, connectionInfo.Port);
                            LogManager.Instance.Write("request connect to : {0},{1}", connectionInfo.IP, connectionInfo.Port);
                        }
                    }
                }
                break;

            //case (int)P2P_Message.SystemMessageType.CONNECT_NOTIFY:
            //{
            //    var connectNotifyMessage = (P2P_Message.SystemMessage)messageData;
            //}
            //break;

            case (int)P2P_Message.SystemMessageType.DISCONNECT:
                {
                    Host.ConnectionList.Remove(connectionId);
                }
                break;
        }

        bool result = MessageDispatcher(hostId, connectionId, channelId, messageId, messageData);
        if (result == false)
        {
            LogManager.Instance.WriteError("MessageEvent hostId:{0} ConnectionId:{1} channelId:{2} messageId:{3} error",
                                           hostId, connectionId, channelId, messageId, messageData);
        }
    }

    protected virtual bool MessageDispatcher(int hostId, int connectionId, int channelId, int messageId, object messageData)
    {
        return true;
    }
}
//using System;
//using System.Net;
//using System.Text;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.Networking.Types;
//using MNF;

//public static class P2P_Message
//{
//    public enum SystemMessageType
//    {
//        CONNECT = -100,
//        CONNECT_NOTIFY,
//        CONNECT_INFO,
//        DISCONNECT,
//    }

//    [Serializable]
//    public class CONNECT_NOTIFY
//    {
//        public ConnectionInfo connectionInfo = new ConnectionInfo();
//    }

//    [Serializable]
//    public class CONNECT_INFO
//    {
//        public List<ConnectionInfo> ConnectionList = new List<ConnectionInfo>();
//    }
//}

//[Serializable]
//public class SystemMessage
//{
//    public int HostId;
//    public int ConnectId;
//    public int ChannelId;

//    public IPEndPoint EndPoint;
//    public NetworkID Network;
//    public NodeID DstNode;

//    public NetworkError NetworkError;
//}

//[Serializable]
//public struct ConnectionInfo
//{
//    public int HostId;
//    public int ConnectionId;
//    public string IP;
//    public string Port;
//    public UInt64 NetworkID;
//    public ushort DstNode;
//}

//public class HostInfo
//{
//    public int HostId { get; set; }
//    public string IP { get; set; }
//    public int Port { get; set; }
//    public ConnectionConfig ConnectionConfig { get; set; }
//    public Dictionary<int, ConnectionInfo> ConnectionList { get; set; }
//    public Dictionary<QosType, int> Channels { get; set; }

//    public HostInfo()
//    {
//        ConnectionList = new Dictionary<int, ConnectionInfo>();
//        Channels = new Dictionary<QosType, int>();
//    }
//}

//public class UNetUtil
//{
///*
//public enum NetworkError
//{
//    Ok,
//    WrongHost,
//    WrongConnection,
//    WrongChannel,
//    NoResources,
//    BadMessage,
//    Timeout,
//    MessageToLong,
//    WrongOperation,
//    VersionMismatch,
//    CRCMismatch,
//    DNSFailure,
//    UsageError
//}
//*/
//    public static bool LogNetworkError(byte error)
//    {
//        if (error == (byte)NetworkError.Ok)
//        {
//            return true;
//        }
//        else
//        {
//            NetworkError nerror = (NetworkError)error;
//            LogManager.Instance.WriteError("Error : {0}", nerror);
//            return false;
//        }
//    }
//}

//public class P2PController : MonoBehaviour
//{
//    enum SPLIT_INDEX
//    {
//        MESSAGE_ID,
//        TYPE_NAME,
//        MESSAGE_JSON,
//    }

//    StringBuilder messageStringBuilder = new StringBuilder(1024 * 100);
//    const int bufferSize = 10240;
//    byte[] recvBuffer = new byte[bufferSize];

//    public HostInfo Host { get; private set; }
//    public int MyConnectionId { get { return 0; } }
//    public int DefaultMaxConnect { get { return 10; } }

//    public bool IsInit { get; private set; }
//    public bool IsServer { get; private set; }

//    public bool Init(string port, bool isServer, QosType[] qosTypes, int maxDefaultConnection)
//    {
//        if (IsInit == true)
//            return true;

//        GlobalConfig gc = new GlobalConfig();
//        gc.ReactorModel = ReactorModel.FixRateReactor;
//        gc.ThreadAwakeTimeout = 10;
//        NetworkTransport.Init(gc);

//        Host = new HostInfo();
//        Host.IP = IPAddress.Any.ToString();
//        Host.Port = Convert.ToInt32(port);
//        Host.ConnectionConfig = new ConnectionConfig();
//        foreach (QosType qosType in qosTypes)
//        {
//            Host.Channels.Add(qosType, Host.ConnectionConfig.AddChannel(qosType));
//        }
//        Host.HostId = NetworkTransport.AddHost(new HostTopology(Host.ConnectionConfig, maxDefaultConnection), Host.Port);

//        IsInit = true;
//        IsServer = isServer;

//        return true;
//    }

//    public bool AddHost(QosType[] qosTypes, int maxDefaultConnection, int hostPort)
//    {
//        var hostInfo = new HostInfo();
//        hostInfo.ConnectionConfig = new ConnectionConfig();
//        foreach (QosType qosType in qosTypes)
//        {
//            hostInfo.Channels.Add(qosType, hostInfo.ConnectionConfig.AddChannel(qosType));
//        }
//        hostInfo.HostId = NetworkTransport.AddHost(new HostTopology(hostInfo.ConnectionConfig, maxDefaultConnection), hostPort);
//        hostInfo.Port = hostPort;

//        return true;
//    }

//    public Dictionary<int, ConnectionInfo>.Enumerator GetConnectList()
//    {
//        return Host.ConnectionList.GetEnumerator();
//    }

//    public void Release()
//    {
//        if (Host == null)
//            return;

//        foreach (var connectionInfo in Host.ConnectionList)
//        {
//            Disconnect(connectionInfo.Value.ConnectionId);
//        }

//        IsInit = false;
//    }

//    protected virtual void OnStartComplete(string myIP, int myPort)
//    {
//    }

//    public void StartServer(string port)
//    {
//        if (LookAround.Instance.IsRunning == true)
//            return;

//        List<string> getPrivateIPList;
//        List<string> getPrivateIPDescList;
//        MNF.Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

//        foreach (var ip in getPrivateIPDescList)
//            LogManager.Instance.Write(ip);

//        if (LookAround.Instance.Start(getPrivateIPList, port, true) == false)
//        {
//            LogManager.Instance.WriteError("LookAround start failed");
//            return;
//        }

//        if (LookAround.Instance.IsRunning == true)
//        {
//            LogManager.Instance.Write("LookAround start(server) success");
//        }
//        else
//        {
//            LogManager.Instance.Write("LookAround start(server) failed");
//            return;
//        }

//        StartCoroutine("RunStartServer");
//    }

//    IEnumerator RunStartServer()
//    {
//        while (LookAround.Instance.IsSetMyInfo == false)
//            yield return new WaitForSeconds(1.0f);

//        QosType[] qosType = { QosType.Reliable, QosType.Unreliable };
//        if (Init(LookAround.Instance.MyPort, true, qosType, DefaultMaxConnect) == false)
//        {
//            LogManager.Instance.WriteError("P2PController Init() failed");
//        }

//        OnStartComplete(LookAround.Instance.GetMyIP().ToString(), Host.Port);
//    }

//    public void StartClient(string port)
//    {
//        if (LookAround.Instance.IsRunning == true)
//            return;

//        List<string> getPrivateIPList;
//        List<string> getPrivateIPDescList;
//        MNF.Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

//        foreach (var ip in getPrivateIPDescList)
//            LogManager.Instance.Write(ip);

//        if (LookAround.Instance.Start(getPrivateIPList, port, false) == false)
//        {
//            LogManager.Instance.WriteError("LookAround start failed");
//            return;
//        }

//        if (LookAround.Instance.IsRunning == true)
//        {
//            LogManager.Instance.Write("LookAround start(client) success");
//        }
//        else
//        {
//            LogManager.Instance.Write("LookAround start(client) failed");
//            return;
//        }

//        StartCoroutine("RunStartClient");
//    }

//    IEnumerator RunStartClient()
//    {
//        while (LookAround.Instance.IsFoundServer == false)
//            yield return new WaitForSeconds(1.0f);

//        LookAround.Instance.Stop();

//        QosType[] qosType = { QosType.Reliable, QosType.Unreliable };
//        if (Init(LookAround.Instance.MyPort, false, qosType, DefaultMaxConnect) == false)
//        {
//            LogManager.Instance.WriteError("P2PController Init() failed");
//        }

//        if (Connect(LookAround.Instance.ServerIP, LookAround.Instance.ServerPort) == true)
//        {
//            LogManager.Instance.Write("P2PController Connect({0}:{1}) succeed",
//                                      LookAround.Instance.ServerIP, LookAround.Instance.ServerPort);
//        }
//        else
//        {
//            LogManager.Instance.WriteError("P2PController Connect({0}:{1}) failed",
//                                           LookAround.Instance.ServerIP, LookAround.Instance.ServerPort);
//        }

//        OnStartComplete(LookAround.Instance.GetMyIP().ToString(), Host.Port);
//    }

//    public bool Connect(string ip, string port)
//    {
//        byte error = 0;
//        var connectionId = NetworkTransport.Connect(Host.HostId, ip, Convert.ToInt32(port), 0, out error);
//        LogManager.Instance.WriteSystem("hostId:{0} Connect:{1} to {2}:{3}", Host.HostId, connectionId, ip, port, error);
//        return (error == (byte)NetworkError.Ok);
//    }

//    public bool IsConnected(int connectionId)
//    {
//        return Host.ConnectionList.ContainsKey(connectionId);
//    }

//    public void Disconnect(int connectionId)
//    {
//        byte error = 0;
//        NetworkTransport.Disconnect(Host.HostId, connectionId, out error);
//        LogManager.Instance.WriteSystem("hostId:{0} Disconnect:{1} error:{2}", Host.HostId, connectionId, error);
//    }

//    public bool SendMessage(int connectionId, QosType channelId, int messageId, object messageData)
//    {
///*
// * Channel {0} for connection {1} does not support fragmented messages; 
// * MTU: {1440}, message length: {3176}, max length {1402}
// */
//        var serializedMessage = Serialize(messageId, messageData);
//        if (serializedMessage == null)
//            return false;

//        byte error = 0;
//        NetworkTransport.Send(Host.HostId, connectionId, Host.Channels[channelId], serializedMessage, serializedMessage.Length, out error);
//        return UNetUtil.LogNetworkError(error);
//    }

//    public bool Broadcast(QosType channelId, int messageId, object messageData, bool loopback = true)
//    {
//        var serializedMessage = Serialize(messageId, messageData);
//        if (serializedMessage == null)
//            return false;

//        foreach (var connectionInfo in Host.ConnectionList)
//        {
//            byte error = 0;
//            NetworkTransport.Send(Host.HostId, connectionInfo.Value.ConnectionId, Host.Channels[channelId], serializedMessage, serializedMessage.Length, out error);
//            UNetUtil.LogNetworkError(error);
//        }

//        if (loopback == true)
//            MessageDispatcher(Host.HostId, MyConnectionId, Host.Channels[channelId], messageId, messageData);

//        return true;
//    }

//    byte[] Serialize<T>(int messageId, T messageData) where T : new()
//    {
//        try
//        {
//            var jsonMessage = JsonSupport.Serialize(messageData);
//            messageStringBuilder.Length = 0;
//            messageStringBuilder.Append(messageId);
//            messageStringBuilder.Append('#');
//            messageStringBuilder.Append(messageData.GetType().FullName);
//            messageStringBuilder.Append('#');
//            messageStringBuilder.Append(jsonMessage);

//            // ... compress lz4
//            //var sendBytes = Encoding.Unicode.GetBytes(jsonMessage);

//            return Encoding.Unicode.GetBytes(messageStringBuilder.ToString());
//        }
//        catch (Exception e)
//        {
//            LogManager.Instance.WriteException(e, "messageId:{0} serialize failed", messageId);
//            return null;
//        }
//    }

//    bool ParseMessage(string recvString, out int messageId, out object messageData)
//    {
//        string[] splitedKey = recvString.Split('#');
//        if (splitedKey.Length != 3)
//        {
//            messageId = 0;
//            messageData = null;
//            return false;
//        }

//        try
//        {
//            messageId = Convert.ToInt32(splitedKey[(int)SPLIT_INDEX.MESSAGE_ID]);
//            Type type = Type.GetType(splitedKey[(int)SPLIT_INDEX.TYPE_NAME]);
//            messageData = JsonSupport.DeSerialize(splitedKey[(int)SPLIT_INDEX.MESSAGE_JSON], type);
//            return true;
//        }
//        catch (Exception e)
//        {
//            LogManager.Instance.WriteException(e, "ParseMessage Failed:{0}", recvString);
//            messageId = 0;
//            messageData = null;
//            return false;
//        }
//    }

//    protected virtual void Update()
//    {
//        if (IsInit == true)
//            ReceiveMessage();
//    }

//    void ReceiveMessage()
//    {
//        int recHostId = 0;
//        int connectionId = 0;
//        int channelId = 0;
//        int dataSize = 0;
//        byte error = 0;

//        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recvBuffer, bufferSize, out dataSize, out error);
//        switch (recData)
//        {
//            case NetworkEventType.Nothing:
//                return;
//            case NetworkEventType.ConnectEvent:
//                {
//                    LogManager.Instance.WriteSystem("ConnectEvent : recData:{0} recHostId:{1} connectionId:{2} channelId:{3} dataSize:{4} error:{5}",
//                                            recData, recHostId, connectionId, channelId, dataSize, (NetworkError)error);

//                    string address;
//                    int port;
//                    NetworkID network;
//                    NodeID dstNode;
//                    NetworkTransport.GetConnectionInfo(recHostId, connectionId, out address, out port, out network, out dstNode, out error);

//                    LogManager.Instance.Write("address:{0} port:{1} network:{2} dstNode:{3} error:{4}",
//                                            address, port, network, dstNode, (NetworkError)error);

//                    var connectMessage = new SystemMessage();
//                    connectMessage.HostId = recHostId;
//                    connectMessage.ConnectId = connectionId;
//                    connectMessage.ChannelId = channelId;
//                    connectMessage.EndPoint = MNF.Utility.GetIPEndPoint(address, port.ToString());
//                    connectMessage.Network = network;
//                    connectMessage.DstNode = dstNode;
//                    connectMessage.NetworkError = (NetworkError)error;
//                    ProcRecviedMessage(recHostId, connectionId, channelId, (int)P2P_Message.SystemMessageType.CONNECT, connectMessage);
//                }
//                break;
//            case NetworkEventType.DataEvent:
//                {
//                    LogManager.Instance.WriteDebug("DataEvent : recData:{0} recHostId:{1} connectionId:{2} channelId:{3} dataSize:{4} error:{5}",
//                                        recData, recHostId, connectionId, channelId, dataSize, (NetworkError)error);

//                    string recvString = Encoding.Unicode.GetString(recvBuffer, 0, dataSize);
//                    int messageId = 0;
//                    object messageData;
//                    if (ParseMessage(recvString, out messageId, out messageData) == true)
//                    {
//                        ProcRecviedMessage(recHostId, connectionId, channelId, messageId, messageData);
//                    }
//                }
//                break;
//            case NetworkEventType.DisconnectEvent:
//                {
//                    LogManager.Instance.WriteSystem("DisconnectEvent : recData:{0} recHostId:{1} connectionId:{2} channelId:{3} dataSize:{4} error:{5}",
//                                        recData, recHostId, connectionId, channelId, dataSize, (NetworkError)error);

//                    var disconnectMessage = new SystemMessage();
//                    disconnectMessage.HostId = recHostId;
//                    disconnectMessage.ConnectId = connectionId;
//                    disconnectMessage.ChannelId = channelId;
//                    disconnectMessage.NetworkError = (NetworkError)error;
//                    ProcRecviedMessage(recHostId, connectionId, channelId, (int)P2P_Message.SystemMessageType.DISCONNECT, disconnectMessage);
//                }
//                break;
//        }
//    }

//    void ProcRecviedMessage(int hostId, int connectionId, int channelId, int messageId, object messageData)
//    {
//        switch (messageId)
//        {
//            case (int)P2P_Message.SystemMessageType.CONNECT:
//                {
//                    var connectMessage = (SystemMessage)messageData;
//                    ConnectionInfo connectionInfo = new ConnectionInfo();
//                    connectionInfo.HostId = hostId;
//                    connectionInfo.ConnectionId = connectionId;
//                    connectionInfo.IP = connectMessage.EndPoint.Address.ToString();
//                    connectionInfo.Port = connectMessage.EndPoint.Port.ToString();

//                    if (IsServer == true)
//                    {
//                        var connectNotify = new P2P_Message.CONNECT_NOTIFY();
//                        connectNotify.connectionInfo = connectionInfo;
//                        Broadcast(QosType.Reliable, (int)P2P_Message.SystemMessageType.CONNECT_NOTIFY, connectNotify, false);

//                        if (Host.ConnectionList.Count > 0)
//                        {
//                            var memberInfo = new P2P_Message.CONNECT_INFO();
//                            foreach (var connectInfo  in Host.ConnectionList)
//                            {
//                                memberInfo.ConnectionList.Add(connectInfo.Value);
//                            }
//                            SendMessage(connectionId, QosType.Reliable, (int)P2P_Message.SystemMessageType.CONNECT_INFO, memberInfo);
//                        }
//                    }
//                    Host.ConnectionList.Add(connectionId, connectionInfo);
//                }
//                break;

//            case (int)P2P_Message.SystemMessageType.CONNECT_INFO:
//                {
//                    if (IsServer == false)
//                    {
//                        var memberInfo = (P2P_Message.CONNECT_INFO)messageData;
//                        foreach (var connectionInfo in memberInfo.ConnectionList)
//                        {
//                            Connect(connectionInfo.IP, connectionInfo.Port);
//                            LogManager.Instance.Write("request connect to : {0},{1}", connectionInfo.IP, connectionInfo.Port);
//                        }
//                    }
//                }
//                break;

//            //case (int)P2P_Message.SystemMessageType.CONNECT_NOTIFY:
//                //{
//                //    var connectNotifyMessage = (P2P_Message.SystemMessage)messageData;
//                //}
//                //break;

//            case (int)P2P_Message.SystemMessageType.DISCONNECT:
//                {
//                    Host.ConnectionList.Remove(connectionId);
//                }
//                break;
//        }

//        bool result = MessageDispatcher(hostId, connectionId, channelId, messageId, messageData);
//        if (result == false)
//        {
//            LogManager.Instance.WriteError("MessageEvent hostId:{0} ConnectionId:{1} channelId:{2} messageId:{3} error",
//                                           hostId, connectionId, channelId, messageId, messageData);
//        }
//    }

//    protected virtual bool MessageDispatcher(int hostId, int connectionId, int channelId, int messageId, object messageData)
//    {
//        return true;
//    }
//}