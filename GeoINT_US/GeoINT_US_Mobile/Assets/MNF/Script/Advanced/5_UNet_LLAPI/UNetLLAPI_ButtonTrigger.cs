using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using MNF;

public class UdpMessageDefine
{
    public enum MESSAGES
    {
        START,
        SPAWN_OBJECT,
        OPERATE_OBJECT,
        P2P_CHAT,
    }

    [Serializable]
    public class PACK_START
    {
    }

    [Serializable]
    public class PACK_SPAWN_OBJECT
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    [Serializable]
    public class PACK_OPERATE_OBJECT
    {
        public bool active;
        public string target_tag;
    }

    [Serializable]
    public class PACK_P2P_CHAT
    {
        public string nickname;
        public string chat;
    }
}

public class UNetLLAPI_ButtonTrigger : P2PController
{
    public Button buttonCreateRoom;
    public Button buttonFindStart;
    public Button buttonFindStop;
    public Button buttonJoin;

    public Button buttonOperate;
    public Button buttonSendChat;
    
    public InputField inputFieldInputChat;
    public InputField inputFieldOutputChat;
    public InputField inputFieldPort;

    public Dropdown dropdown;

    public Rigidbody rocketInstance;
    public float lastInpuDelta;

    GameObject controlObject;
    bool activeFlag = true;
    bool isRunning = true;
    string serverIP = "";

    void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter(), (int)ENUM_LOG_TYPE.LOG_TYPE_ALL);
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");
    }

    public void OnServer()
	{
        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        MNF.Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

        if (IPPulse.Instance.IsRunning == false)
        {
            if (IPPulse.Instance.Start(getPrivateIPList, inputFieldPort.text, true) == false)
            {
                LogManager.Instance.WriteError("IPPulse.Instance.Start() failed");
            }
            IPPulse.Instance.Pause();
            if (StartServer(inputFieldPort.text) == true)
            {
                LogManager.Instance.Write("start success Port[{0}]", inputFieldPort.text);
            }
            else
            {
                LogManager.Instance.WriteError("start failed Port[{0}]", inputFieldPort.text);
            }
            isRunning = true;
            StartCoroutine("UpdateDeviceList");

            buttonFindStart.transform.gameObject.SetActive(false);
            buttonFindStop.transform.gameObject.SetActive(false);
            buttonJoin.transform.gameObject.SetActive(false);
        }
    }

    public void OnClient()
    {
        if (serverIP == "")
        {
            LogManager.Instance.WriteError(string.Format("Not found server!"));
            return;
        }

        LogManager.Instance.Write(string.Format("Connect to {0}:{1}", serverIP, inputFieldPort.text));
        StartClient(serverIP, inputFieldPort.text);

        buttonCreateRoom.transform.gameObject.SetActive(false);
    }

    public void OnFindStart()
    {
        buttonCreateRoom.gameObject.SetActive(false);

        IPPulse.Instance.ClearDeviceList();

        if (IPPulse.Instance.IsRunning == true)
        {
            IPPulse.Instance.Resume();
        }
        else
        {
            List<string> getPrivateIPList;
            List<string> getPrivateIPDescList;
            MNF.Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);

            if (IPPulse.Instance.Start(getPrivateIPList, inputFieldPort.text, false) == false)
            {
                LogManager.Instance.WriteError("IPPulse.Instance.Start() failed");
            }

            isRunning = true;
            StartCoroutine("UpdateDeviceList");
        }
    }

    public void OnFindStop()
    {
        IPPulse.Instance.Pause();
    }

    IEnumerator UpdateDeviceList()
    {
        while (isRunning == true)
        {
            dropdown.options.Clear();

            //IPPulse.Instance.RemoveTimeoutDevice(30);
            var deviceList = IPPulse.Instance.GetDeviceList();
            foreach (var deviceInfo in deviceList)
            {
                var optionData = new Dropdown.OptionData(deviceInfo.ToString());
                dropdown.options.Add(optionData);

                if (deviceInfo.isServer == true && serverIP == "")
                {
                    serverIP = deviceInfo.waitIPEndPoint.Address.ToString();
                    LogManager.Instance.Write("Found Server:{0}", serverIP);
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    public void OnOperate()
    {
        if (activeFlag == true)
            activeFlag = false;
        else
            activeFlag = true;

        var operateObject = new UdpMessageDefine.PACK_OPERATE_OBJECT();
        operateObject.active = activeFlag;
        operateObject.target_tag = "object";
        Broadcast(QosType.Reliable, (int)UdpMessageDefine.MESSAGES.OPERATE_OBJECT, operateObject);
    }

    public void OnSendChat()
    {
        var p2pChat = new UdpMessageDefine.PACK_P2P_CHAT();
        p2pChat.nickname = MNF.Utility.GetProcessID().ToString();
        p2pChat.chat = inputFieldInputChat.text;
        Broadcast(QosType.Reliable, (int)UdpMessageDefine.MESSAGES.P2P_CHAT, p2pChat);
    }

    protected override void Update()
    {
        base.Update();

        lastInpuDelta += Time.deltaTime;

        if ((Input.GetMouseButton(0) == true) || (Input.touchCount > 0))
            BroadcastSpawn();
    }

    void OnGUI()
    {
        if (IsInit == true)
        {
            var connectionEnum = GetConnectList();
            while (connectionEnum.MoveNext())
            {
                var connectionInfo = connectionEnum.Current.Value;
                GUILayout.Label(string.Format("IP:{0} Port:{1} ConId:{2}",
                                              connectionInfo.IP, connectionInfo.Port, connectionInfo.ConnectionId));
            }
        }
    }

    void BroadcastSpawn()
    {
        if (IsInit == false)
            return;
        
        if (lastInpuDelta < 0.5f)
            return;

        lastInpuDelta = 0;

        Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.y = Camera.main.transform.position.y;

        var spawnObject = new UdpMessageDefine.PACK_SPAWN_OBJECT();
        spawnObject.position = worldPos;
        spawnObject.rotation = new Quaternion();
        Broadcast(QosType.Reliable, (int)UdpMessageDefine.MESSAGES.SPAWN_OBJECT, spawnObject);
    }

    protected override bool MessageDispatcher(int hostId, int connectionId, int channelId, int messageId, object messageData)
    {
        LogManager.Instance.Write("hostId:{0} connectionId:{1} channelId:{2} messageId:{3}", hostId, connectionId, channelId, messageId);

        switch ((UdpMessageDefine.MESSAGES)messageId)
        {
            case UdpMessageDefine.MESSAGES.START:
                {
                }
                break;

            case UdpMessageDefine.MESSAGES.SPAWN_OBJECT:
                {
                    var spawn = (UdpMessageDefine.PACK_SPAWN_OBJECT)messageData;
                    Instantiate(rocketInstance, spawn.position, spawn.rotation);
                }
                break;

            case UdpMessageDefine.MESSAGES.OPERATE_OBJECT:
                {
                    var operate = (UdpMessageDefine.PACK_OPERATE_OBJECT)messageData;
                    var targetObject = GameObject.FindGameObjectWithTag(operate.target_tag);

                    if (controlObject == null)
                        controlObject = targetObject;

                    controlObject.SetActive(operate.active);

                    if (operate.active == true)
                        buttonOperate.GetComponentInChildren<Text>().text = "Disable";
                    else
                        buttonOperate.GetComponentInChildren<Text>().text = "Enable";
                }
                break;

            case UdpMessageDefine.MESSAGES.P2P_CHAT:
                {
                    var p2pChat = (UdpMessageDefine.PACK_P2P_CHAT)messageData;
                    inputFieldOutputChat.text += p2pChat.nickname + " : " + p2pChat.chat + "\n";
                }
                break;
        }
        return true;
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        IPPulse.Instance.Stop();
        LogManager.Instance.Release();
        Release();
        NetworkTransport.Shutdown();
    }
}