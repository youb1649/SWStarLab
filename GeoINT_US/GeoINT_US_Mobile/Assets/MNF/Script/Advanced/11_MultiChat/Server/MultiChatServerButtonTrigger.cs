using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MNF;

public class MultiChatServerButtonTrigger : MonoBehaviour
{
    bool isRun = false;
    bool isReleased = false;
    int clientToServerPort = 0;
    int serverToServerPort = 0;

    public Dropdown clientDropdown;
    public Dropdown serverDropdown;
    public InputField inputServerIP;
    public InputField inputServerPort;

    public Button buttonChatServerStart;
    public Button buttonChatServerStop;

    public void Awake()
    {
		LogManager.Instance.SetLogWriter(new UnityLogWriter());
		if (LogManager.Instance.Init() == false)
			Debug.Log("LogWriter init failed");

        // for test
        if (PlayerPrefs.GetInt("ClientToServerPort", 0) == 0)
            PlayerPrefs.SetInt("ClientToServerPort", 10000);

        if (PlayerPrefs.GetInt("ServerToServerPort", 0) == 0)
            PlayerPrefs.SetInt("ServerToServerPort", 50000);

        buttonChatServerStart.gameObject.SetActive(true);
        buttonChatServerStop.gameObject.SetActive(false);
    }

	void Release()
	{
        if (isReleased == true)
            return;

        isRun = false;
        isReleased = true;

        Debug.Log("Application ending after " + Time.time + " seconds");

        Debug.Log("IPPulse.Instance.Stop");
        IPPulse.Instance.Stop();

        Debug.Log("TcpHelper.Instance.StopAccept");
        TcpHelper.Instance.StopAccept();

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

        var responseMessageList = IPPulse.Instance.PopReponseMessage();
        foreach (var responseMessage in responseMessageList)
        {
            if ((responseMessage.IsMine == false) && (responseMessage.IsFromServer == true))
            {
                var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
                while (clientSessionEnumerator.MoveNext())
                {
                    clientSessionEnumerator.Current.Value.AsyncSend(responseMessage.ID, responseMessage.Message);
                }
            }
        }
    }

    public void OnStopServer()
    {
        Release();

        inputServerIP.text = "";
        inputServerPort.text = "";

        buttonChatServerStart.gameObject.SetActive(true);
        buttonChatServerStop.gameObject.SetActive(false);
    }

    public void OnStartServer()
    {
        clientToServerPort = PlayerPrefs.GetInt("ClientToServerPort", 0);
        serverToServerPort = PlayerPrefs.GetInt("ServerToServerPort", 0);
        LogManager.Instance.Write("ClientToServerPort:{0}, ServerToServerPort:{1}",
            clientToServerPort, serverToServerPort);

        LogManager.Instance.Write("Start IPPulse ..");

        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        foreach (var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, clientToServerPort.ToString(), true) == false)
        {
            LogManager.Instance.WriteError("IPPulse start failed");
            return;
        }

        LogManager.Instance.Write("Start TcpHelper ..");
        if (TcpHelper.Instance.Start(false) == false)
        {
			LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
			return;
		}

        inputServerIP.text = string.Join(",", IPPulse.Instance.GetNetworkIPList());
        inputServerPort.text = clientToServerPort.ToString();

        LogManager.Instance.Write("Start StartAccept ..");
        if (TcpHelper.Instance.StartAccept<MultiChatServerSession, MultiChatServerMessageDispatcher>(inputServerPort.text, 10) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<MultiChatServerSession, MultiChatServerMessageDispatcher>() failed");
			return;
		}
        LogManager.Instance.Write("Start Server Success");

        isRun = true;
        isReleased = false;
        StartCoroutine("UpdateServerList");

        buttonChatServerStart.gameObject.SetActive(false);
        buttonChatServerStop.gameObject.SetActive(true);
    }

    IEnumerator UpdateServerList()
    {
        while (isRun == true)
        {
            LogManager.Instance.Write("Refresh server list..");
            serverDropdown.options.Clear();

            int timeLimitSec = 30;
            IPPulse.Instance.RemoveTimeoutDevice(timeLimitSec);

            if (IPPulse.Instance.GetDeviceList().Count > 0)
            {
                IPPulse.Instance.Pause();
            }
            else
            {
                IPPulse.Instance.Resume();
            }

            foreach (var deviceInfo in IPPulse.Instance.GetDeviceList())
            {
                if (deviceInfo.isServer == false)
                    continue;

                var optionData = new UnityEngine.UI.Dropdown.OptionData(deviceInfo.ToString());
                serverDropdown.options.Add(optionData);
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    public static MultiChatServerButtonTrigger FindMultiChatServerButtonTrigger()
    {
        var foundButtonTrigger = GameObject.Find("EventSystem").GetComponentsInChildren<MultiChatServerButtonTrigger>();
        return foundButtonTrigger[0];
    }
}
