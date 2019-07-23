using System;
using System.Collections;
using UnityEngine;
using MNF;
using MNF_Common;

public class MNF_EchoServer : UnitySingleton<MNF_EchoServer>
{
    public bool IsInit { get; private set; }
    public EchoServerScene EchoServerScenePoint { get; set; }
    bool isRunHeartBeat = true;

    public bool Init()
    {
        if (IsInit == true)
            return true;

		if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            return false;
        }

        if (TcpHelper.Instance.StartAccept<EchoServerBinarySession, EchoServerBinaryMessageDispatcher>(EchoServerScenePoint.binaryServerPort.text, 500) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<BinaryServerSession, BinaryMessageDispatcher_Server>() failed");
            return false;
        }

        if (TcpHelper.Instance.StartAccept<EchoServerJsonSession, EchoServerJsonMessageDispatcher>(EchoServerScenePoint.jsonServerPort.text, 500) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<JsonServerSession, JsonMessageDispatcher_Server>() failed");
            return false;
        }

		StartCoroutine("HeartBeat");

		IsInit = true;

        return IsInit;
	}

	IEnumerator HeartBeat()
	{
		while (isRunHeartBeat == true)
		{
			int binaryHeartBeatCount = 0;
			int jsonHeartBeatCount = 0;
			var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
			while (clientSessionEnumerator.MoveNext())
			{
                var session = clientSessionEnumerator.Current.Value;
                if (session.GetType() == typeof(EchoServerBinarySession))
                {
                    var message = new BinaryMessageDefine.PACK_SC_HEARTBEAT_REQ();
					message.tickCount = Environment.TickCount;
					session.AsyncSend((int)BinaryMessageDefine.ENUM_SC_.SC_HEARTBEAT_REQ, message);
                    ++binaryHeartBeatCount;
                }
				else if (session.GetType() == typeof(EchoServerJsonSession))
				{
                    var message = new JsonMessageDefine.PACK_SC_JSON_HEARTBEAT_REQ();
                    message.tickCount = Environment.TickCount;
                    session.AsyncSend((int)JsonMessageDefine.ENUM_SC_.SC_JSON_HEARTBEAT_REQ, message);
					++jsonHeartBeatCount;
				}
			}
            LogManager.Instance.Write("HeartBeat sent Binary:{0}, Json:{1}", binaryHeartBeatCount, jsonHeartBeatCount);
			yield return new WaitForSeconds(5.0f);
		}
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
        isRunHeartBeat = false;
		IPPulse.Instance.Stop();
        TcpHelper.Instance.StopAccept();
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}
}