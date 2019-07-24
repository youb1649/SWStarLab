using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MNF;
using MNF_Common;

public class MultiChatClientButtonTrigger : MonoBehaviour
{
    bool isReleased = false;
    int clientToServerPort = 0;
    int serverToServerPort = 0;
    string nickName = "";

    public InputField inputServerIP;
    public InputField inputServerPort;
    public InputField inputNickname;
    public InputField inputChatMessage;
    public InputField inputRecvChatMessage;

    public Button buttonChatClientStart;
    public Button buttonChatClientStop;
    public Button buttonSendChatMessage;

    public void Awake()
    {
        Debug.Log("LogManager.Instance.SetLogWriter");
		LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
			Debug.Log("LogWriter init failed");

        // for test
        if (PlayerPrefs.GetInt("ClientToServerPort", 0) == 0)
            PlayerPrefs.SetInt("ClientToServerPort", 10000);

        if (PlayerPrefs.GetInt("ServerToServerPort", 0) == 0)
            PlayerPrefs.SetInt("ServerToServerPort", 50000);

        if (PlayerPrefs.GetString("ClientName", "").Length == 0)
            PlayerPrefs.SetString("ClientName", "Client_1_1");

        buttonChatClientStop.gameObject.SetActive(false);
        buttonSendChatMessage.gameObject.SetActive(false);
    }

	void Release()
	{
        if (isReleased == true)
            return;

        isReleased = true;

        Debug.Log("Application ending after " + Time.time + " seconds");

        Debug.Log("IPPulse.Instance.Stop");
        IPPulse.Instance.Stop();

        Debug.Log("TcpHelper.Instance.Stop");
        TcpHelper.Instance.Stop();
	}

	void OnDestroy()
	{
        Release();
	}

	void OnApplicationQuit()
	{
		Release();

        Debug.Log("LogManager.Instance.Release");
        LogManager.Instance.Release();
	}

	void Update()
	{
		TcpHelper.Instance.DipatchNetworkInterMessage();
	}

    public void OnStopClient()
    {
        Release();

        inputServerIP.text = "";
        inputServerPort.text = "";
        inputNickname.text = "";
        inputChatMessage.text = "";
        inputRecvChatMessage.text = "";

        buttonChatClientStart.gameObject.SetActive(true);
        buttonChatClientStop.gameObject.SetActive(false);
        buttonSendChatMessage.gameObject.SetActive(false);
    }

    public void OnStartClient()
    {
        clientToServerPort = PlayerPrefs.GetInt("ClientToServerPort", 0);
        serverToServerPort = PlayerPrefs.GetInt("ServerToServerPort", 0);
        inputNickname.text = PlayerPrefs.GetString("ClientName", "");

        LogManager.Instance.Write("ClientToServerPort:{0}, ServerToServerPort:{1}, ClientName:{2}",
            clientToServerPort, serverToServerPort, nickName);

        buttonChatClientStart.gameObject.SetActive(false);

        StartCoroutine("StartClient");
    }

    IEnumerator StartClient()
    {
        LogManager.Instance.Write("Start IPPulse ..");

        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, clientToServerPort.ToString(), false) == false)
        {
            LogManager.Instance.WriteError("IPPulse start failed");
            yield break;
        }

        const bool onlyServer = true;
        string outIP = "";
        while (IPPulse.Instance.FindDevice(clientToServerPort, onlyServer, out outIP) == false)
        {
            LogManager.Instance.Write("Looking for Server port[{0}]", clientToServerPort);
            yield return new WaitForSeconds(1.0f);
        }
        IPPulse.Instance.Pause();

        LogManager.Instance.Write("Start TcpHelper..");
        if (TcpHelper.Instance.Start(false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            yield break;
        }

        LogManager.Instance.Write("Start AsyncConnect to ip[{0}] port[{1}]", outIP, clientToServerPort);
        if (TcpHelper.Instance.AsyncConnect<MultiChatClientSession, MultiChatClientMessageDispatcher>(
            outIP, clientToServerPort.ToString()) == null)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect({0}:{1}) failed",
                inputServerIP.text, inputServerPort.text);
            yield break;
        }
        LogManager.Instance.Write("Start Client Success");

        inputServerIP.text = outIP;
        inputServerPort.text = clientToServerPort.ToString();

        buttonChatClientStop.gameObject.SetActive(true);
        buttonSendChatMessage.gameObject.SetActive(true);

        isReleased = false;
    }

    public void OnSendChatMessage()
    {
        if (inputChatMessage.text.Length == 0)
            return;

        var chatMessage = new MultiChatMessageDefine.PACK_CHAT();
        chatMessage.chat = inputNickname.text + " => " + inputChatMessage.text;

        var clientSession = TcpHelper.Instance.GetFirstClient<MultiChatClientSession>();
        clientSession.AsyncSend((int)MultiChatMessageDefine.MESSAGES.CHAT, chatMessage);
    }

    public static MultiChatClientButtonTrigger FindMultiChatClientButtonTrigger()
    {
        var foundButtonTrigger = GameObject.Find("EventSystem").GetComponentsInChildren<MultiChatClientButtonTrigger>();
        return foundButtonTrigger[0];
    }
}
