using System;
using UnityEngine;
using System.Collections;
using MNF;
using MNF_Common;

public class ScreenCapture_JPG : MonoBehaviour
{
    public int frameRate = 20;
    public int jpgQuality = 25;

    Texture2D capturedTexture;
    Rect readPixels;

    void Start()
    {
        capturedTexture = new Texture2D(Screen.width / 2, Screen.height / 2, TextureFormat.RGB24, false);
        readPixels = new Rect(0, 0, Screen.width / 2, Screen.height / 2);
    }

    void Update()
    {
        Time.captureFramerate = frameRate;
        StartCoroutine(Capture());
    }

	IEnumerator Capture()
	{
        // frame avrage : 14~15 fps, jpg
		// We should only read the screen after all rendering is complete
		yield return new WaitForEndOfFrame();

		// Read screen contents into the texture
		capturedTexture.ReadPixels(readPixels, 0, 0, false);
		capturedTexture.Apply();

		// Encode texture into jpg
        byte[] jpgBytes = capturedTexture.EncodeToJPG(jpgQuality);
		
        //Create new thread then save image to file
        new System.Threading.Thread(() =>
        {
            var screenShare = new ScreenShareMessageDefine.PACK_SC_SCREEN_SHARE();
            Buffer.BlockCopy(jpgBytes, 0, screenShare.binary, 0, jpgBytes.Length);
            screenShare.sendSize = jpgBytes.Length;

            var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
            while (clientSessionEnumerator.MoveNext())
            {
                clientSessionEnumerator.Current.Value.AsyncSend(
                    (int)ScreenShareMessageDefine.ENUM_SC_.SC_SCREEN_SHARE, screenShare);
            }
        }).Start();
	}
}