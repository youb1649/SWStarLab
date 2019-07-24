using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class ScreenShareClientButtonTrigger : MonoBehaviour
{
    public InputField serverIP;
    public InputField serverPort;

    public void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");
    }

    void Release()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        IPPulse.Instance.Stop();
        TcpHelper.Instance.Stop();
        LogManager.Instance.Release();
    }

    void OnDestroy()
    {
        Release();
    }

    void OnApplicationQuit()
    {
        Release();
    }

    void Update()
    {
        TcpHelper.Instance.DipatchNetworkInterMessage();
    }

    public void OnStartClient()
    {
        StartCoroutine("StartClient");
    }
    
    IEnumerator StartClient()
    {
        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, false) == false)
        {
            LogManager.Instance.WriteError("LookAround start failed");
            yield break;
        }

        const bool onlyServer = true;
        string outIP = "";
        while (IPPulse.Instance.FindDevice(Convert.ToInt32(serverPort.text), onlyServer, out outIP) == false)
        {
            LogManager.Instance.Write("Looking for Server port[{0}]", serverPort.text);
            yield return new WaitForSeconds(1.0f);
        }
        IPPulse.Instance.Pause();

        serverIP.text = outIP;

        if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            yield break;
        }

        if (TcpHelper.Instance.AsyncConnect<ScreenShareClientSession, ScreenShareClientMessageDispatcher>(
            serverIP.text, serverPort.text) == null)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect({0}:{1}) failed",
                serverIP.text, serverPort.text);
            yield break;
        }

        LogManager.Instance.Write("Start Client Success");
    }
}
