using MNF;

public class LoginServerSession : JsonSession
{
    public override int OnAccept()
    {
        LogManager.Instance.Write("OnAccept : {0}:{1}", ToString(), GetType());

        TcpHelper.Instance.AddClientSession(GetHashCode(), this);

        return 0;
    }

    public override int OnDisconnect()
    {
        LogManager.Instance.Write("onDisconnect : {0}:{1}", ToString(), GetType());

        TcpHelper.Instance.RemoveClientSession(this);

        return 0;
    }
}