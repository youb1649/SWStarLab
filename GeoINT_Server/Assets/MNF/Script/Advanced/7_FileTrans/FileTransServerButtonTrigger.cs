using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;
using MNF_Common;

public class FileTransServerButtonTrigger : MonoBehaviour
{
    public InputField serverIP;
    public InputField serverPort;
    public RawImage rawImage;
    public Button sendButton;
    public float waitForSec = 0.005f;

    string loadFileName = "FileTransImage";
    TextAsset loadBinaryImage = null;
    bool isSend = false;

    public void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter(), (int)ENUM_LOG_TYPE.LOG_TYPE_ALL);
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");

        sendButton.gameObject.SetActive(false);
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

    void FixedUpdate()
    {
        TcpHelper.Instance.DipatchNetworkInterMessage();
    }

    public void OnStartServer()
    {
        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);
        serverIP.text = string.Join(",", getPrivateIPList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, true) == false)
        {
            LogManager.Instance.WriteError("LookAround start failed");
            return;
        }
        IPPulse.Instance.Pause();

        if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            return;
        }

        if (TcpHelper.Instance.StartAccept<FileTransServerSession, FileTransServerMessageDispatcher>(serverPort.text, 500) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<FileTransServerSession, FileTransServerMessageDispatcher>() failed");
            return;
        }

        sendButton.gameObject.SetActive(true);
        LogManager.Instance.Write("Start Server Success");
    }

    public void OnLoadImage()
    {
        if (loadBinaryImage != null)
            return;

		LogManager.Instance.Write("Start Load Image");

		loadBinaryImage = Resources.Load(loadFileName, typeof(TextAsset)) as TextAsset;

        var texture = new Texture2D(200, 100);
		texture.LoadImage(loadBinaryImage.bytes);
        rawImage.texture = texture;

        var text = rawImage.GetComponentInChildren<Text>();
        text.text = string.Format("w:{0} - h:{1} - s:{2}",
            rawImage.texture.width, rawImage.texture.height, loadBinaryImage.bytes.Length);
	
        LogManager.Instance.Write("End Load Image");
	}

	public void OnSendImage()
	{
        if (isSend == false)
        {
            StartCoroutine("RunSendImage");
            isSend = true;
        }
	}

	IEnumerator RunSendImage()
	{
        var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
		while (clientSessionEnumerator.MoveNext())
		{
			var session = clientSessionEnumerator.Current.Value;

			// start
			var fileTransStart = new FileTransMessageDefine.PACK_SC_FILE_TRANS_START();
			fileTransStart.fileName = loadFileName;
			fileTransStart.fileSize = loadBinaryImage.bytes.Length;
			session.SyncSend((int)FileTransMessageDefine.ENUM_SC_.SC_FILE_TRANS_START, fileTransStart);
			LogManager.Instance.Write("Start Send Image - file:{0} - size:{1}", fileTransStart.fileName, fileTransStart.fileSize);

			var text = rawImage.GetComponentInChildren<Text>();

            // send
            bool disconnectedSession = false;
			int sentSize = 0;
			while (sentSize < loadBinaryImage.bytes.Length)
			{
                if ((session.IsConnected == false) || (session.IsNotifyDisconnect == true))
                {
                    LogManager.Instance.Write("Disconnected Session:{0} in sending", session);
                    disconnectedSession = true;
                    break;
                }

                var fileTransSend = new FileTransMessageDefine.PACK_SC_FILE_TRANS_SEND();
				fileTransSend.sendSize = fileTransSend.binary.Length;
				if ((sentSize + fileTransSend.sendSize) > loadBinaryImage.bytes.Length)
					fileTransSend.sendSize = loadBinaryImage.bytes.Length - sentSize;

				Buffer.BlockCopy(loadBinaryImage.bytes, sentSize, fileTransSend.binary, 0, fileTransSend.sendSize);
				sentSize += fileTransSend.sendSize;

				session.SyncSend((int)FileTransMessageDefine.ENUM_SC_.SC_FILE_TRANS_SEND, fileTransSend);

				text.text = string.Format("Sending Image - sentSize:{0}", sentSize);
                yield return new WaitForSeconds(waitForSec);
            }

            if (disconnectedSession == false)
            {
                // end
                var fileTransEnd = new FileTransMessageDefine.PACK_SC_FILE_TRANS_END();
                fileTransEnd.fileName = loadFileName;
                session.SyncSend((int)FileTransMessageDefine.ENUM_SC_.SC_FILE_TRANS_END, fileTransEnd);
                LogManager.Instance.Write("End Send Image - file:{0}", fileTransEnd.fileName);
            }
		}

        isSend = false;
        yield break;
    }
}
