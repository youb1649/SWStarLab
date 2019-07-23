using MNF;

public class EchoServerJsonSession : JsonSession
{
    public override int OnAccept()
    {
        LogManager.Instance.Write("OnAccept : {0}:{1}", this.ToString(), this.GetType());
		TcpHelper.Instance.AddClientSession(this.GetHashCode(), this);
		return 0;
    }

    public override int OnDisconnect()
    {
        LogManager.Instance.Write("onDisconnect : {0}:{1}", this.ToString(), this.GetType());
		TcpHelper.Instance.RemoveClientSession(this);
		return 0;
    }
}