using MNF_Common;
using MNF;

public class EchoClientJsonMessageDispatcher : DefaultDispatchHelper<EchoClientJsonSession, JsonMessageDefine, JsonMessageDefine.ENUM_SC_>
{
    int onSC_JSON_ECHO(EchoClientJsonSession session, object message)
    {
        MNF_EchoClient.Instance.EchoClientScenePoint.updateJsonProfiling();

        var jsonEcho = new JsonMessageDefine.PACK_CS_JSON_ECHO();
        jsonEcho.sandwiches = Sandwich.CreateSandwichList(5);
        session.AsyncSend((int)JsonMessageDefine.ENUM_CS_.CS_JSON_ECHO, jsonEcho);

        return 0;
	}

	int onSC_JSON_HEARTBEAT_REQ(EchoClientJsonSession session, object message)
	{
		var heartbeatReq = (JsonMessageDefine.PACK_SC_JSON_HEARTBEAT_REQ)message;
		LogManager.Instance.Write("{0}, {1}, {2}", session, heartbeatReq.GetType(), heartbeatReq.tickCount);

		var heartbeatRes = new JsonMessageDefine.PACK_CS_JSON_HEARTBEAT_RES();
        heartbeatRes.tickCount = heartbeatReq.tickCount;
		session.AsyncSend((int)JsonMessageDefine.ENUM_CS_.CS_JSON_HEARTBEAT_RES, heartbeatRes);

		return 0;
	}
}