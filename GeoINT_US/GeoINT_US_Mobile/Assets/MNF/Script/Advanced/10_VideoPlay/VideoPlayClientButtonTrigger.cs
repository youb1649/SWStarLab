using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;
using MNF_Common;

public class VideoPlayClientButtonTrigger : MonoBehaviour
{
    bool isRunning = false;
    DateTime lastReceivedTime;

	public InputField serverIP;
	public InputField serverPort;

    VideoPlayClientSession session = null;

    public void Awake()
    {
		LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");
	}

    void Release()
    {
        isRunning = false;
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
        List<DeviceInfo> deviceList = IPPulse.Instance.GetDeviceList(onlyServer);
        while (deviceList.Count == 0)
        {
            LogManager.Instance.Write("Looking for Server..");
            deviceList = IPPulse.Instance.GetDeviceList(onlyServer);
            yield return new WaitForSeconds(1.0f);
        }
        IPPulse.Instance.Pause();

        serverIP.text = deviceList[0].waitIPEndPoint.Address.ToString();
        serverPort.text = deviceList[0].waitIPEndPoint.Port.ToString();

        if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            yield break;
        }

        if (ConnectToServer() == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect({0}:{1}) failed",
                serverIP.text, serverPort.text);
            yield break;
        }

        LogManager.Instance.Write("Start Client Success");

        isRunning = true;
        StartCoroutine("CheckHeartbeat");
    }

    IEnumerator CheckHeartbeat()
    {
        lastReceivedTime = DateTime.Now;

        while (isRunning == true)
        {
            if (lastReceivedTime >= DateTime.Now.AddSeconds(-5.0))
            {
                if ((session != null) && (session.IsConnected == true))
                {
                    var ping = new VideoPlayMessageDefine.PACK_CS_PING();
                    session.AsyncSend((int)VideoPlayMessageDefine.ENUM_CS_.CS_PING, ping);
                    LogManager.Instance.WriteDebug("send ping to server");
                }

                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                // disconnect from server
                if (session != null)
                {
                    LogManager.Instance.WriteDebug("disconnect from server");
                    session.Disconnect();
                    session = null;
                    LogManager.Instance.WriteDebug("waitting 5 sec ...");
                    yield return new WaitForSeconds(5.0f);

                    LogManager.Instance.WriteDebug("reconnect to server");
                    if (ConnectToServer() == true)
                    {
                        LogManager.Instance.WriteDebug("reconnect request successed");
                        lastReceivedTime = DateTime.Now;
                    }
                    else
                    {
                        LogManager.Instance.WriteError("reconnect request failed");
                    }
                }
            }
        }
    }

    public void UpdateHeartbeatCheckTime()
    {
        lastReceivedTime = DateTime.Now;
        LogManager.Instance.WriteError("update heartbeat check time");
    }

    public bool ConnectToServer()
    {
        session = TcpHelper.Instance.AsyncConnect<VideoPlayClientSession, VideoPlayClientMessageDispatcher>(
            serverIP.text, serverPort.text);

        return (session != null);
    }
}
