using MNF_Common;
using MNF;

public class EchoClientBinaryMessageDispatcher : DefaultDispatchHelper<EchoClientBinarySession, BinaryMessageDefine, BinaryMessageDefine.ENUM_SC_>
{
    int onSC_ECHO(EchoClientBinarySession session, object message)
    {
        MNF_EchoClient.Instance.EchoClientScenePoint.updateBinaryProfiling(MarshalHelper.GetManagedDataSize(message));

        var ecshoPacket = new BinaryMessageDefine.PACK_CS_ECHO();
        session.AsyncSend((int)BinaryMessageDefine.ENUM_CS_.CS_ECHO, ecshoPacket);

        return 0;
	}

    int onSC_HEARTBEAT_REQ(EchoClientBinarySession session, object message)
	{
		var heartbeatReq = (BinaryMessageDefine.PACK_SC_HEARTBEAT_REQ)message;
		LogManager.Instance.Write("{0}, {1}, {2}", session, heartbeatReq.GetType(), heartbeatReq.tickCount);

		var heartbeatRes = new BinaryMessageDefine.PACK_CS_HEARTBEAT_RES();
        heartbeatRes.tickCount = heartbeatReq.tickCount;
		session.AsyncSend((int)BinaryMessageDefine.ENUM_CS_.CS_HEARTBEAT_RES, heartbeatRes);

		return 0;
	}
}