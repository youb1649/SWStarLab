using MNF;

public class MNFServerManagerSession : JsonSession
{
    public override int OnAccept()
    {
        LogManager.Instance.Write("OnAccept : {0}:{1}", this.ToString(), this.GetType());
        return 0;
    }

    public override int OnDisconnect()
    {
        LogManager.Instance.Write("OnDisconnect : {0}:{1}", this.ToString(), this.GetType());
        return 0;
    }
}
