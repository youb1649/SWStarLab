using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;
using MNF_Common;

public class IPPulseEventTrigger : MonoBehaviour
{
    public Dropdown dropdown;
    public Toggle toggle;
    public InputField inputFieldChatMessage;
    public InputField inputFieldPort;
    public InputField recvMessage;

    public Button buttonStart;
    public Button buttonPause;
    public Button buttonResume;
    public Button buttonSend;
    public Button buttonClear;

    bool isRunning = true;
    List<string> recvMessageList;

    public void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");

        recvMessageList = new List<string>();

        buttonStart.gameObject.SetActive(true);
        buttonPause.gameObject.SetActive(false);
        buttonResume.gameObject.SetActive(false);
        buttonSend.gameObject.SetActive(false);
        buttonClear.gameObject.SetActive(false);
    }

    public void OnStart()
    {
        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        foreach(var ip in getPrivateIPDescList)
            LogManager.Instance.Write(ip);

        if (IPPulse.Instance.Start(getPrivateIPList, inputFieldPort.text, toggle.isOn) == false)
        {
            LogManager.Instance.WriteError("IPPulse start failed");
            return;
        }

        isRunning = true;
        StartCoroutine("UpdateDeviceList");

        buttonStart.gameObject.SetActive(false);
        buttonPause.gameObject.SetActive(true);
        buttonResume.gameObject.SetActive(false);
    }

    public void OnPause()
    {
        LogManager.Instance.Write("Pause IPPulse...");
        IPPulse.Instance.Pause();

        buttonPause.gameObject.SetActive(false);
        buttonResume.gameObject.SetActive(true);
        buttonSend.gameObject.SetActive(true);
        buttonClear.gameObject.SetActive(true);
    }

    public void OnResume()
    {
        LogManager.Instance.Write("Resume IPPulse...");
        IPPulse.Instance.Resume();

        buttonPause.gameObject.SetActive(true);
        buttonResume.gameObject.SetActive(false);
        buttonSend.gameObject.SetActive(false);
        buttonClear.gameObject.SetActive(false);
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        Debug.Log("Application ending after " + Time.time + " seconds");
        IPPulse.Instance.Stop();
        LogManager.Instance.Release();
    }

    void UpdateRecvMessage(string from, string nickname, string chat)
    {
        recvMessageList.Insert(0, string.Format("[{0}] {1} : {2}", from, nickname, chat));
        if (recvMessageList.Count > 5)
            recvMessageList.RemoveAt(0);

        recvMessage.text = string.Join("\n", recvMessageList);
    }

    public void OnSendMessage()
	{
        if (inputFieldChatMessage.text.Length == 0)
            return;

        if (isRunning == false)
            return;

        var chat = new IPPulseMessageDefine.PACK_CHAT
        {
            nickname = Utility.GetProcessID().ToString(),
            chat = inputFieldChatMessage.text
        };
        IPPulse.Instance.SendMessage((int)IPPulseMessageDefine.MESSAGES.CHAT, chat);

        UpdateRecvMessage("Send", chat.nickname, chat.chat);
    }

    public void OnClearMessage()
    {
        recvMessageList.Clear();
        recvMessage.text = "";
    }

    IEnumerator UpdateDeviceList()
    {
        while (isRunning == true)
        {
            dropdown.options.Clear();

            IPPulse.Instance.RemoveTimeoutDevice(20);
            var deviceList = IPPulse.Instance.GetDeviceList();
            foreach (var deviceInfo in deviceList)
            {
                var optionData = new Dropdown.OptionData(deviceInfo.ToString());
                dropdown.options.Add(optionData);
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    void Update()
    {
        var responseMessageList = IPPulse.Instance.PopReponseMessage();
        foreach (var responseMessage in responseMessageList)
        {
            recvMessage.text = string.Format("{0} {1} {2} {3}",
                responseMessage.ID, responseMessage.IsMine, responseMessage.Type, responseMessage.Message);

            switch ((IPPulseMessageDefine.MESSAGES)responseMessage.ID)
            {
                case IPPulseMessageDefine.MESSAGES.CHAT:
                    {
                        var chat = (IPPulseMessageDefine.PACK_CHAT)responseMessage.Message;
                        UpdateRecvMessage("Recv", chat.nickname, chat.chat);
                    }
                    break;
            }
        }
    }
}