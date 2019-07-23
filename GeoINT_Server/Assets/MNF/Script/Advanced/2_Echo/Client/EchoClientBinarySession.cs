using MNF_Common;
using MNF;

public class EchoClientBinarySession : BinarySession
{
    public override int OnConnectSuccess()
    {
        LogManager.Instance.Write("onConnectSuccess : {0}:{1}", this.ToString(), this.GetType());

        var ecshoPacket = new BinaryMessageDefine.PACK_CS_ECHO();
        AsyncSend((int)BinaryMessageDefine.ENUM_CS_.CS_ECHO, ecshoPacket);

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