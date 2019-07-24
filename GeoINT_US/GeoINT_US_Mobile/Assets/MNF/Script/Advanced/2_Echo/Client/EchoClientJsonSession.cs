using MNF_Common;
using MNF;

public class EchoClientJsonSession : JsonSession
{
    public override int OnConnectSuccess()
    {
        LogManager.Instance.Write("onConnectSuccess : {0}:{1}", this.ToString(), this.GetType());

        var jsonEcho = new JsonMessageDefine.PACK_CS_JSON_ECHO();
        jsonEcho.sandwiches = Sandwich.CreateSandwichList(10);
        AsyncSend((int)JsonMessageDefine.ENUM_CS_.CS_JSON_ECHO, jsonEcho);

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