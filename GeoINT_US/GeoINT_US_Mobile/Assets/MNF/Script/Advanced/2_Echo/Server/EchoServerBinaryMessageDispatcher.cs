using System;
using MNF_Common;
using MNF;

public class EchoServerBinaryMessageDispatcher : DefaultDispatchHelper<EchoServerBinarySession, BinaryMessageDefine, BinaryMessageDefine.ENUM_CS_>
{
    int onCS_ECHO(EchoServerBinarySession session, object message)
    {
        MNF_EchoServer.Instance.EchoServerScenePoint.updateBinaryProfiling(MarshalHelper.GetManagedDataSize(message));

        BinaryMessageDefine.PACK_SC_ECHO echoPacket = new BinaryMessageDefine.PACK_SC_ECHO();
        session.AsyncSend((int)BinaryMessageDefine.ENUM_SC_.SC_ECHO, echoPacket);

        return 0;
	}

    int onCS_HEARTBEAT_RES(EchoServerBinarySession session, object message)
	{
        var resMessage = (BinaryMessageDefine.PACK_CS_HEARTBEAT_RES)message;

        // check tickcount
        var elapsedTick = Environment.TickCount - resMessage.tickCount;
        LogManager.Instance.Write("HeartBeat reponse : {0}", elapsedTick);

		return 0;
	}
}