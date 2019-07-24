using System;
using UnityEngine;
using System.Collections;
using MNF;
using MNF_Common;

public class ScreenCapture_RenderTexture : MonoBehaviour
{
    public int frameRate = 20;
    public int jpgQuality = 25;

    Texture2D capturedTexture;
    RenderTexture rt;

    void Start()
    {
        Time.captureFramerate = frameRate;
        capturedTexture = new Texture2D(Screen.width / 2, Screen.height / 2, TextureFormat.RGB24, false);
        rt = new RenderTexture(Screen.width / 2, Screen.height / 2, 24);
    }

    void Update()
    {
        Time.captureFramerate = frameRate;
        StartCoroutine(Capture());
    }

	IEnumerator Capture()
	{
        // frame avrage : 11~12 fps
        yield return new WaitForEndOfFrame();

        // Render from all!
        foreach (Camera cam in Camera.allCameras)
        {
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = null;
        }

        RenderTexture.active = rt;
        capturedTexture.ReadPixels(new Rect(0, 0, Screen.width / 2, Screen.height / 2), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;

        //Split the process up
        yield return 0;

        // Encode texture into JPG
        byte[] dataBytes = capturedTexture.EncodeToJPG(jpgQuality);

        //Create new thread then save image to file
        new System.Threading.Thread(() =>
        {
            var screenShare = new ScreenShareMessageDefine.PACK_SC_SCREEN_SHARE();
            Buffer.BlockCopy(dataBytes, 0, screenShare.binary, 0, dataBytes.Length);
            screenShare.sendSize = dataBytes.Length;

            var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
            while (clientSessionEnumerator.MoveNext())
            {
                clientSessionEnumerator.Current.Value.AsyncSend(
                    (int)ScreenShareMessageDefine.ENUM_SC_.SC_SCREEN_SHARE, screenShare);
            }
        }).Start();
	}
}