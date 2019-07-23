using UnityEngine;
using MNF;

public class MNF_ChatServer : UnitySingleton<MNF_ChatServer>
{
    public bool IsInit { get; private set; }
    public BinaryChatServerScene BinaryChatServerScenePoint { get; set; }

    public bool init()
    {
        if (IsInit == true)
            return true;

		if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            return false;
        }

		if (TcpHelper.Instance.StartAccept<ChatServerSession, ChatServerMessageDispatcher>(BinaryChatServerScenePoint.serverPort.text, 500) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<ChatServerSession, BinaryMessageDispatcher_ChatServer>() failed");
            return false;
        }

        IsInit = true;

        return IsInit;
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
		TcpHelper.Instance.StopAccept();
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}
}