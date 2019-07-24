using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MNF;

public class VideoPlay : MonoBehaviour
{
    UnityEngine.Video.VideoPlayer videoPlayer;

    private void Awake()
    {
        string url = "file://" + Application.streamingAssetsPath + "/" + "VideoExample.webm";

#if !UNITY_EDITOR && UNITY_ANDROID
                url = Application.streamingAssetsPath + "/" + "VideoExample.webm";
#endif

        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("VideoCamera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 0.5F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        videoPlayer.url = url;

        // Skip the first 100 frames.
        videoPlayer.frame = 100;

        // Restart from beginning when done.
        videoPlayer.isLooping = true;

        videoPlayer.targetCameraAlpha = 1f;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        videoPlayer.loopPointReached += EndReached;
    }

    void Update()
    {
    }

    public TimeSpan CalcDelayTime(DateTime reservedTime)
    {
        if (reservedTime <= DateTime.Now)
        {
            StartCoroutine("OnDelayedPlay", 0);
            return new TimeSpan(0);
        }

        return (reservedTime - DateTime.Now);
    }

    public void OnPlay(DateTime reservedTime)
    {
        TimeSpan interval = CalcDelayTime(reservedTime);
        StartCoroutine("OnDelayedPlay", interval.TotalMilliseconds);
		LogManager.Instance.Write("Play => Now:{0}, Play:{1}, Interval:{2}, PlayTime:{3}", DateTime.Now, reservedTime, interval.TotalMilliseconds, videoPlayer.time);
    }

    IEnumerator OnDelayedPlay()
    {
        videoPlayer.Play();
        yield return null;
    }

    public void OnPause(DateTime reservedTime)
    {
        TimeSpan interval = CalcDelayTime(reservedTime);
        StartCoroutine("OnDelayedPause", interval.Milliseconds);
		LogManager.Instance.Write("Pause => Now:{0}, Pause:{1}, Interval:{2}, PlayTime:{3}", DateTime.Now, reservedTime, interval.TotalMilliseconds, videoPlayer.time);
    }

    IEnumerator OnDelayedPause()
    {
        videoPlayer.Pause();
        yield return null;
    }

    public void OnStop(DateTime reservedTime)
    {
        TimeSpan interval = CalcDelayTime(reservedTime);
        StartCoroutine("OnDelayedStop", interval.Milliseconds);
		LogManager.Instance.Write("Stop => Now:{0}, Stop:{1}, Interval:{2}, PlayTime:{3}", DateTime.Now, reservedTime, interval.TotalMilliseconds, videoPlayer.time);
    }

    IEnumerator OnDelayedStop()
    {
        videoPlayer.Stop();
        yield return null;
    }

    public void SetPlayTime(double playTime)
    {
        videoPlayer.time = playTime;
    }

    public double GetPlayTime()
    {
        return videoPlayer.time;
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        //vp.playbackSpeed = vp.playbackSpeed / 10.0F;
        vp.Stop();
    }
}
