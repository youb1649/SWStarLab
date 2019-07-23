using MNF_Common;
using MNF;

public class VideoPlayServerMessageDispatcher : DefaultDispatchHelper<VideoPlayServerSession, VideoPlayMessageDefine, VideoPlayMessageDefine.ENUM_CS_>
{
    int onCS_PING(VideoPlayServerSession session, object message)
    {
        var pong = new VideoPlayMessageDefine.PACK_SC_PONG();
        session.AsyncSend((int)VideoPlayMessageDefine.ENUM_SC_.SC_PONG, pong);

        return 0;
    }
}