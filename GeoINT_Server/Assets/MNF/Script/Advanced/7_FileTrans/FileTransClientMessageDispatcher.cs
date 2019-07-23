using System;
using UnityEngine;
using UnityEngine.UI;
using MNF_Common;
using MNF;

public class FileTransClientMessageDispatcher : DefaultDispatchHelper<FileTransClientSession, FileTransMessageDefine, FileTransMessageDefine.ENUM_SC_>
{
    int onSC_FILE_TRANS_START(FileTransClientSession session, object message)
    {
        var fileTransStart = (FileTransMessageDefine.PACK_SC_FILE_TRANS_START)message;
        session.rawImageBytes = new byte[fileTransStart.fileSize];
        session.recvedSize = 0;

		var rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();
		var text = rawImage.GetComponentInChildren<Text>();
        text.text = string.Format("Start File Trans : {0}", fileTransStart.fileName);
		
        return 0;
    }

    int onSC_FILE_TRANS_SEND(FileTransClientSession session, object message)
    {
		var fileTransSend = (FileTransMessageDefine.PACK_SC_FILE_TRANS_SEND)message;
        Buffer.BlockCopy(fileTransSend.binary, 0, session.rawImageBytes, session.recvedSize, fileTransSend.sendSize);
        session.recvedSize += fileTransSend.sendSize;

		var rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();
		var text = rawImage.GetComponentInChildren<Text>();
        text.text = string.Format("Total Recved Size : {0}/bytes", session.recvedSize);
		return 0;
	}

	int onSC_FILE_TRANS_END(FileTransClientSession session, object message)
	{
		var fileTransEnd = (FileTransMessageDefine.PACK_SC_FILE_TRANS_END)message;

        var rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();
        var text = rawImage.GetComponentInChildren<Text>();
        text.text = string.Format("file:{0} - s:{1}/bytes", fileTransEnd.fileName, session.rawImageBytes.Length);

        var eventSystemTrigger = GameObject.Find("EventSystem").GetComponent<FileTransClientButtonTrigger>();
        eventSystemTrigger.DrawImage(session.rawImageBytes);

		return 0;
	}
}