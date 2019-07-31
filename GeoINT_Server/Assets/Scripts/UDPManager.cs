using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPManager : MonoBehaviour
{
    public int localPort = 5394;
    public int remotePort = 5395;
    public string remoteIP = "192.168.0.208";
    public Queue<string> queMsg = new Queue<string>();

    private UdpClient socket = null;
    private object objLock = new object();

    void Start()
    {
        socket = new UdpClient(localPort);
        socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
        Debug.Log("Udp Script Started.");
    }

    void OnUdpData(IAsyncResult result)
    {
        UdpClient socket = result.AsyncState as UdpClient;
        IPEndPoint source = new IPEndPoint(0, 0);
        byte[] message = socket.EndReceive(result, ref source);
        if (null != message)
        {
            lock (objLock)
            {
                queMsg.Enqueue(System.Text.Encoding.UTF8.GetString(message));
            }
        }

        socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
    }

    public string GetMsg()
    {
        string szRawMsg = "";
        if (0 == queMsg.Count)
            return szRawMsg;

        lock (objLock)
        {
            szRawMsg = queMsg.Dequeue();
        }

        return szRawMsg;
    }

    public void SendMsg(string szMsg)
    {
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
        byte[] message = Encoding.UTF8.GetBytes(szMsg);
        if (null != socket)
            socket.Send(message, message.Length, target);
        else
            Debug.LogError("Socket Object is null.");
    }
}
