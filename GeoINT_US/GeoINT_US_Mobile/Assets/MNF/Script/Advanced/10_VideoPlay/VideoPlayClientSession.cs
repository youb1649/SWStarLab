using MNF;
using UnityEngine;
using MNF_Common;

public class VideoPlayClientSession : JsonSession
{
    public override int OnConnectSuccess()
	{
		LogManager.Instance.Write("onConnectSuccess : {0}:{1}", this.ToString(), this.GetType());
		return 0;
	}

	public override int OnConnectFail()
	{
		LogManager.Instance.Write("onConnectFail : {0}:{1}", this.ToString(), this.GetType());
		return 0;
	}

	public override int OnDisconnect()
	{
		LogManager.Instance.Write("onDisconnect : {0}:{1}", this.ToString(), this.GetType());
        return 0;
	}
}