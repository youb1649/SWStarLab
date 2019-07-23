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
        public int iPort = 7268;
        public GeoINTServerLogEvent thisEvent = new GeoINTServerLogEvent();

        void InsertLog(string szMsg)
        {

        }
     
        // Use this for initialization
        void Start()
        {
            thisEvent.AddListener(InsertLog);
            NetworkServer.Listen(iPort);
            NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
            NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            NetworkServer.RegisterHandler(MyMsgType.sMsg, OnMessageProcess);
            WriteLog("Server Started !!");
        }

        public void OnConnected(NetworkMessage netMsg)
        {
            lstConn.Add(netMsg.conn);
            WriteLog("Connected : [" + netMsg.conn.address + "]");
        }

        public void OnDisconnected(NetworkMessage netMsg)
        {
            lstConn.Remove(netMsg.conn);
            WriteLog("Disconnected : [" + netMsg.conn.address + "]");
        }

        public void OnMessageProcess(NetworkMessage netMsg)
        {
            string szTmp = netMsg.ReadMessage<StringMessage>().szMsg;
            WriteLog(szTmp);

            //StringMessage sm = new StringMessage();
            //sm.szMsg = "Sent ForwardMsg to Server!";
            //netMsg.conn.Send(MyMsgType.sMsg, sm);
        }

        private void WriteLog(string szMsg)
        {
            if(null != thisEvent)
                thisEvent.Invoke(szMsg);
        }
    }
}