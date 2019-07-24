using MNF;

public class ChatClientSession : BinarySession
{
    public override int OnConnectSuccess()
    {
        LogManager.Instance.Write("onConnectSuccess : {0}:{1}", ToString(), GetType());
        TcpHelper.Instance.AddClientSession(GetHashCode(), this);
        return 0;
    }

    public override int OnConnectFail()
    {
        LogManager.Instance.Write("onConnectFail : {0}:{1}", ToString(), GetType());
        return 0;
    }

    public override int OnDisconnect()
    {
        LogManager.Instance.Write("onDisconnect : {0}:{1}", ToString(), GetType());
        TcpHelper.Instance.RemoveClientSession(this);
        return 0;
    }
}