using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System;

namespace GeoINT_Server
{
    [System.Serializable]
    public class GeoINTServerLogEvent : UnityEvent<string>
    {
    }

    public class PacketRelayManager : NetworkManager
    {
        public class MyMsgType
        {
            public static short sMsg = MsgType.Highest + 3;
        };

        public class StringMessage : MessageBase
        {
            public string szMsg;
        }

        public List<NetworkConnection> lstConn = new List<NetworkConnection>();
        public int iPort = 72681;
        public GeoINTServerLogEvent thisEvent = null;

        void InsertLog(string szMsg)
        {

        }
     
        void Awake()
        {
            thisEvent = new GeoINTServerLogEvent();
            thisEvent.AddListener(InsertLog);
        }

        // Use this for initialization
        void Start()
        {
            NetworkServer.Listen(iPort);
            NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
            NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            NetworkServer.RegisterHandler(MyMsgType.sMsg, OnMessageProcess);

        }

        public void OnConnected(NetworkMessage netMsg)
        {
            lstConn.Add(netMsg.conn);
            Debug.Log("Client connected : [" + netMsg.conn.address + "]");
        }

        public void OnDisconnected(NetworkMessage netMsg)
        {
            lstConn.Remove(netMsg.conn);
            Debug.Log("Client Disconnected : [" + netMsg.conn.address + "]");
        }

        public void OnMessageProcess(NetworkMessage netMsg)
        {
            string szTmp = netMsg.ReadMessage<StringMessage>().szMsg;
            StringMessage sm = new StringMessage();
            sm.szMsg = "Sent ForwardMsg to Server!";
            netMsg.conn.Send(MyMsgType.sMsg, sm);
        }

        private void WriteLog(string szMsg)
        {
            if(null != thisEvent)
                thisEvent.Invoke(szMsg);
        }
    }
}