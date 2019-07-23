using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;
using MNF_Common;

public class MNFServerBase : MonoBehaviour
{
    public string Port;
    public string SessionClassName;
    public string MessageDispatcherClassName;

    void OnDestroy()
    {
        Release();
    }

    void OnApplicationQuit()
    {
        Release();
    }

    void FixedUpdate()
    {
        TcpHelper.Instance.DipatchNetworkInterMessage();
    }

    void Release()
    {
        TcpHelper.Instance.StopAccept();
        TcpHelper.Instance.Stop();
    }

    public void OnStart()
    {
        if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            return;
        }

        int backLogCount = 10;
        if (TcpHelper.Instance.StartAccept(SessionClassName, MessageDispatcherClassName, Port.ToString(), backLogCount) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept({0}, {1}, {2}, {3}) failed", SessionClassName, MessageDispatcherClassName, Port, backLogCount);
            return;
        }

        LogManager.Instance.Write("Start Server Success");
    }
}
