using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;
using MNF_Common;

public class VideoPlayServerButtonTrigger : MonoBehaviour
{
	public InputField serverIP;
	public InputField serverPort;

    VideoPlay videoPlayer = null;

    #region UNITY
    void Awake()
    {
		LogManager.Instance.SetLogWriter(new UnityLogWriter());
		if (LogManager.Instance.Init() == false)
			Debug.Log("LogWriter init failed");

        var foundVideoPlayObject = GameObject.Find("VideoPlayer").GetComponentsInChildren<VideoPlay>();
        if (foundVideoPlayObject.Length == 0)
            return;

        videoPlayer = foundVideoPlayObject[0];
    }

    void Release()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
        IPPulse.Instance.Stop();
		TcpHelper.Instance.StopAccept();
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}

	void OnDestroy()
	{
		Release();
	}

	void OnApplicationQuit()
	{
		Release();
	}

	void Update()
	{
		TcpHelper.Instance.DipatchNetworkInterMessage();
	}
    #endregion

    #region BUTTON
    public void OnStartServer()
    {
        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, true) == false)
        {
            LogManager.Instance.WriteError("LookAround start failed");
            return;
        }
        IPPulse.Instance.Pause();

        serverIP.text = string.Join(",", IPPulse.Instance.GetNetworkIPList());

        if (TcpHelper.Instance.Start(false) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
			return;
		}

        if (TcpHelper.Instance.StartAccept<VideoPlayServerSession, VideoPlayServerMessageDispatcher>(serverPort.text, 500) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<VideoPlayServerSession, VideoPlayServerMessageDispatcher>() failed");
			return;
		}
		LogManager.Instance.Write("Start Server Success");
	}

    public DateTime GetDelayedTime()
    {
        const int DELAY_TIME = 1500;
        DateTime now = DateTime.Now;
        TimeSpan duration = new TimeSpan(0, 0, 0, 0, DELAY_TIME);
        return now.Add(duration);
    }

    public void SendVideoControlMessage(VideoPlayMessageDefine.VIDEO_CONTROL videoControlValue, DateTime reservedTime)
    {
        var videoControl = new VideoPlayMessageDefine.PACK_SC_VIDEO_CONTROL();
        videoControl.control = videoControlValue;
        videoControl.reservedTime = Utility.ConvertToUnixTime(reservedTime);
        videoControl.playTime = videoPlayer.GetPlayTime();

        var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
        while (clientSessionEnumerator.MoveNext())
        {
            clientSessionEnumerator.Current.Value.AsyncSend(
                (int)VideoPlayMessageDefine.ENUM_SC_.SC_VIDEO_CONTROL, videoControl);
        }
    }

    public void OnVideoPlay()
    {
        var delayedTime = GetDelayedTime();
        SendVideoControlMessage(VideoPlayMessageDefine.VIDEO_CONTROL.PLAY, delayedTime);
        videoPlayer.OnPlay(delayedTime);
    }

    public void OnVideoPause()
	{
        var delayedTime = GetDelayedTime();
        SendVideoControlMessage(VideoPlayMessageDefine.VIDEO_CONTROL.PAUSE, delayedTime);
        videoPlayer.SetPlayTime(3);
        videoPlayer.OnPause(delayedTime);
    }

    public void OnVideoStop()
	{
        var delayedTime = GetDelayedTime();
        SendVideoControlMessage(VideoPlayMessageDefine.VIDEO_CONTROL.STOP, delayedTime);
        videoPlayer.OnStop(delayedTime);
    }
    #endregion
}
