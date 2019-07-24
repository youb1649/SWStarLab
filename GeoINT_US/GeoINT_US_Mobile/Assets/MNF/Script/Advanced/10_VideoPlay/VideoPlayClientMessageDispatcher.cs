using System;
using UnityEngine;
using UnityEngine.UI;
using MNF_Common;
using MNF;

public class VideoPlayClientMessageDispatcher : DefaultDispatchHelper<VideoPlayClientSession, VideoPlayMessageDefine, VideoPlayMessageDefine.ENUM_SC_>
{
    VideoPlay videoPlayer = null;

    int onSC_VIDEO_CONTROL(VideoPlayClientSession session, object message)
    {
        var foundVideoPlayObject = GameObject.Find("VideoPlayer").GetComponentsInChildren<VideoPlay>();
        if (foundVideoPlayObject.Length == 0)
            return 0;
        videoPlayer = foundVideoPlayObject[0];

        var videoControl = (VideoPlayMessageDefine.PACK_SC_VIDEO_CONTROL)message;
        var convertedTime = Utility.UnixTimeToDateTime(videoControl.reservedTime);
        LogManager.Instance.Write("control:{0} reservedTime:({1} -> {2}) NowTime:{3} PlayTime:{4}", videoControl.control, videoControl.reservedTime, convertedTime, DateTime.Now, videoControl.playTime);

        switch (videoControl.control)
        {
            case VideoPlayMessageDefine.VIDEO_CONTROL.PLAY:
                videoPlayer.SetPlayTime(videoControl.playTime);
                videoPlayer.OnPlay(convertedTime);
                break;

            case VideoPlayMessageDefine.VIDEO_CONTROL.PAUSE:
                videoPlayer.SetPlayTime(videoControl.playTime);
                videoPlayer.OnPause(convertedTime);
                break;

            case VideoPlayMessageDefine.VIDEO_CONTROL.STOP:
                videoPlayer.OnStop(convertedTime);
                break;
        }
        return 0;
    }

    int onSC_PONG(VideoPlayClientSession session, object message)
    {
        var foundEventSystemButtonTrrigerObject = GameObject.Find("EventSystem").GetComponentsInChildren<VideoPlayClientButtonTrigger>();
        if (foundEventSystemButtonTrrigerObject.Length == 0)
            return 0;

        var eventSystemButtonTrriger = foundEventSystemButtonTrrigerObject[0];
        eventSystemButtonTrriger.UpdateHeartbeatCheckTime();

        return 0;
    }
}