using System;
using MNF_Common;
using MNF;

public class EchoServerJsonMessageDispatcher : DefaultDispatchHelper<EchoServerJsonSession, JsonMessageDefine, JsonMessageDefine.ENUM_CS_>
{
    int onCS_JSON_ECHO(EchoServerJsonSession session, object message)
    {
        MNF_EchoServer.Instance.EchoServerScenePoint.updateJsonProfiling();

        var echo = (JsonMessageDefine.PACK_CS_JSON_ECHO)message;
        var jsonEcho = new JsonMessageDefine.PACK_SC_JSON_ECHO();
        jsonEcho.sandwiches = echo.sandwiches;
        session.AsyncSend((int)JsonMessageDefine.ENUM_SC_.SC_JSON_ECHO, jsonEcho);

        return 0;
	}

	int onCS_JSON_HEARTBEAT_RES(EchoServerJsonSession session, object message)
	{
        var resMessage = (JsonMessageDefine.PACK_CS_JSON_HEARTBEAT_RES)message;

		// check tickcount
		var elapsedTick = Environment.TickCount - resMessage.tickCount;
		LogManager.Instance.Write("HeartBeat reponse : {0}", elapsedTick);

		return 0;
	}
}
