using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class ScreenShareServerButtonTrigger : MonoBehaviour
{
	public InputField serverIP;
	public InputField serverPort;
    public MonoBehaviour screenCapture;

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
		TcpHelper.Instance.StopAccept();
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

    public void OnStartServer()
    {
        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);
        serverIP.text = string.Join(",", getPrivateIPList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, true) == false)
        {
            LogManager.Instance.WriteError("LookAround start failed");
            return;
        }
        IPPulse.Instance.Pause();

        if (TcpHelper.Instance.Start(false) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
			return;
		}

        if (TcpHelper.Instance.StartAccept<ScreenShareServerSession, ScreenShareServerMessageDispatcher>(serverPort.text, 500) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<ScreenShareServerSession, ScreenShareServerMessageDispatcher>() failed");
			return;
		}
		LogManager.Instance.Write("Start Server Success");
	}
}
