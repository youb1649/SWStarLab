using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class BinaryChatClientScene : IScene
{
	public InputField serverIP;
	public InputField serverPort;
    public InputField inputChatMessageBox;
    public InputField recvMessageBox;
    public InputField inputNicknameBox;

    public BinaryChatClientScene()
    {
        SceneName = "BinaryChatClientScene";
    }

    void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");

        MNF_ChatClient.Instance.BinaryChatClientScenePoint = this;

        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, false) == false)
            LogManager.Instance.WriteError("LookAround start failed");
	}

    void Start()
    {
        inputChatMessageBox.GetComponentInChildren<Text>().text = "";
        recvMessageBox.GetComponentInChildren<Text>().text = "";
    }
}