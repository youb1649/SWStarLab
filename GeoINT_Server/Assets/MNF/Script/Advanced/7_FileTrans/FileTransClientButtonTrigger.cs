using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class FileTransClientButtonTrigger : MonoBehaviour
{
	public InputField serverIP;
	public InputField serverPort;
    public RawImage rawImage;

	string loadFileName = "DefaultImage";
	TextAsset loadBinaryImage = null;

    public void Awake()
    {
		LogManager.Instance.SetLogWriter(new UnityLogWriter(), (int)ENUM_LOG_TYPE.LOG_TYPE_ALL);
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");
	}

    void Release()
    {
		Debug.Log("Application ending after " + Time.time + " seconds");
		IPPulse.Instance.Stop();
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

    public void OnStartClient()
    {
        StartCoroutine("StartClient");
    }

    IEnumerator StartClient()
    {
        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, false) == false)
        {
            LogManager.Instance.WriteError("LookAround start failed");
            yield break;
        }

        const bool onlyServer = true;
        string outIP = "";
        while (IPPulse.Instance.FindDevice(Convert.ToInt32(serverPort.text), onlyServer, out outIP) == false)
        {
            LogManager.Instance.Write("Looking for Server port[{0}]", serverPort.text);
            yield return new WaitForSeconds(1.0f);
        }
        IPPulse.Instance.Pause();

        serverIP.text = outIP;

        if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            yield break;
        }

        if (TcpHelper.Instance.AsyncConnect<FileTransClientSession, FileTransClientMessageDispatcher>(
            serverIP.text, serverPort.text) == null)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect({0}:{1}) failed",
                serverIP.text, serverPort.text);
            yield break;
        }

        LogManager.Instance.Write("Start Client Success");
    }

    public void OnLoadImage()
    {
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

    public void DrawImage(byte[] rawImageBytes)
    {
        StartCoroutine("RunDrawImage", rawImageBytes);
    }

    IEnumerator RunDrawImage(byte[] rawImageBytes)
    {
        var texture = new Texture2D(200, 100);
        texture.LoadImage(rawImageBytes);
        rawImage.texture = texture;
        yield break;
    }
}
