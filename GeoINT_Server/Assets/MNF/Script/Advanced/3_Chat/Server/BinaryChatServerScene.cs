using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class BinaryChatServerScene : IScene
{
	public InputField serverIP;
	public InputField serverPort;
    public InputField inputChatMessageBox;
    public InputField recvMessageBox;
    public InputField inputNicknameBox;
    public Dropdown dropdownSessins;

    public BinaryChatServerScene()
    {
        SceneName = "BinaryChatServerScene";
    }

    void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");

        MNF_ChatServer.Instance.BinaryChatServerScenePoint = this;

        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);
        serverIP.text = string.Join(", ", getPrivateIPList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, true) == false)
            LogManager.Instance.WriteError("LookAround start failed");
        IPPulse.Instance.Pause();
    }

    void Start()
    {
        inputChatMessageBox.GetComponentInChildren<Text>().text = "";
        recvMessageBox.GetComponentInChildren<Text>().text = "";
    }
}