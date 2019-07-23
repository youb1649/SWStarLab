using System;
using UnityEngine;
using System.Collections;
using MNF;
using MNF_Common;

public class ScreenCapture_EXR : MonoBehaviour
{
    public int frameRate = 10;
    public int exrFlag;

    Texture2D capturedTexture;
    Rect readPixels;
    RenderTexture rt;

    void Start()
    {
        exrFlag = (int)Texture2D.EXRFlags.None;

        capturedTexture = new Texture2D(Screen.width / 2, Screen.height / 2, TextureFormat.RGBAFloat, false);
        readPixels = new Rect(0, 0, Screen.width / 2, Screen.height / 2);
        rt = new RenderTexture(Screen.width / 2, Screen.height / 2, 24);
    }

    void Update()
    {
        Time.captureFramerate = frameRate;
        StartCoroutine(Capture());
    }

	IEnumerator Capture()
	{
        // frame avrage : 7~8 fps, exr
        yield return new WaitForEndOfFrame();

        // Read screen contents into the texture
        Graphics.SetRenderTarget(rt);
        capturedTexture.ReadPixels(readPixels, 0, 0, false);
        capturedTexture.Apply();

        // Encode texture into jpg
        if ((int)Texture2D.EXRFlags.CompressPIZ < exrFlag)
        {
            exrFlag = (int)Texture2D.EXRFlags.None;
            Debug.Log("Invalid exFlag");
        }
        byte[] jpgBytes = capturedTexture.EncodeToEXR((Texture2D.EXRFlags)exrFlag);

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