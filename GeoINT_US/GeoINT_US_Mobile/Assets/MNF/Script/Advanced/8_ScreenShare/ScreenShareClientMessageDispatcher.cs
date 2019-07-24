using System;
using UnityEngine;
using UnityEngine.UI;
using MNF_Common;
using MNF;

public class ScreenShareClientMessageDispatcher : DefaultDispatchHelper<ScreenShareClientSession, ScreenShareMessageDefine, ScreenShareMessageDefine.ENUM_SC_>
{
    RawImage rawImage;
	Texture2D texture;
    public byte[] rawImageBytes;

    int onSC_SCREEN_SHARE(ScreenShareClientSession session, object message)
    {
        if (texture == null)
            texture = new Texture2D(100, 100);
        
        if (rawImage == null)
            rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();

        if (rawImageBytes == null)
            rawImageBytes = new byte[ScreenShareMessageDefine.MaxImageSize];

		var screenShare = (ScreenShareMessageDefine.PACK_SC_SCREEN_SHARE)message;
        Buffer.BlockCopy(screenShare.binary, 0, rawImageBytes, 0, screenShare.sendSize);

		texture.LoadImage(rawImageBytes);
		rawImage.texture = texture;

		var text = rawImage.GetComponentInChildren<Text>();
        text.text = string.Format("w:{0} - h:{1} - s:{2}", rawImage.texture.width, rawImage.texture.height, screenShare.sendSize);

		return 0;
	}
}