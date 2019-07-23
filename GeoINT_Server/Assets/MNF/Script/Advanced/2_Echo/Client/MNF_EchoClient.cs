using System.Collections;
using UnityEngine;
using MNF;

public class MNF_EchoClient : UnitySingleton<MNF_EchoClient>
{
    public bool IsInit { get; private set; }
    public EchoClientScene EchoClientScenePoint { get; set; }

    public bool OnStart()
    {
        if (IsInit == true)
            return true;

        IsInit = true;
        StartCoroutine("StartClient");

        return IsInit;
    }

    IEnumerator StartClient()
    {
        IsInit = false;

        const bool onlyServer = true;
        string outIP = "";
        while (IPPulse.Instance.FindDevice(999, onlyServer, out outIP) == false)
        {
            LogManager.Instance.Write("Looking for Server...");
            yield return new WaitForSeconds(1.0f);
        }
        IPPulse.Instance.Pause();
        EchoClientScenePoint.serverIP.text = outIP;

        if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            yield break;
        }

        if (TcpHelper.Instance.AsyncConnect<EchoClientBinarySession, EchoClientBinaryMessageDispatcher>(
            EchoClientScenePoint.serverIP.text, EchoClientScenePoint.binaryServerPort.text) == null)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect({0}:{1}) failed",
                EchoClientScenePoint.serverIP.text, EchoClientScenePoint.binaryServerPort.text);
            yield break;
        }

        if (TcpHelper.Instance.AsyncConnect<EchoClientJsonSession, EchoClientJsonMessageDispatcher>(
            EchoClientScenePoint.serverIP.text, EchoClientScenePoint.jsonServerPort.text) == null)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect({0}:{1}) failed",
                EchoClientScenePoint.serverIP.text, EchoClientScenePoint.jsonServerPort.text);
            yield break;
        }

        IsInit = true;
    }

    void Update()
    {
        if (IsInit == false)
            return;

        TcpHelper.Instance.DipatchNetworkInterMessage();
    }

	void OnApplicationQuit()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
        IPPulse.Instance.Stop();
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}
}