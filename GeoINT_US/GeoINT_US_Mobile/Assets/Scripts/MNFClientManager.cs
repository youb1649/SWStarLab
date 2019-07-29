using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class MNFClientManager : MonoBehaviour
{
    public Text tmLeft = null;
    private string szLeft = "";

    public Text tmRight = null;
    private string szRight = "";

    void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");

        if (TcpHelper.Instance.Start(isRunThread: false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            return;
        }

        if (TcpHelper.Instance.AsyncConnect<MNFClientManagerSession, MNFClientManagerMessageDispatcher>("192.168.0.134", "10001") == null)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect() failed");
            return;
        }
    }

    void Update()
    {
        TcpHelper.Instance.DipatchNetworkInterMessage();
    }

    void FixedUpdate()
    {
        UpdateControlPostion();
    }

    void UpdateControlPostion()
    {
        if(szLeft != tmLeft.text || szRight != tmRight.text)
        {
            szLeft = tmLeft.text;
            szRight = tmRight.text;

            var varServer = new MNFClientManagerBasicMessageDefine.PACK_CS_JoystickPosition();
            varServer.msg = szLeft +"|"+ szRight;
            var session = TcpHelper.Instance.GetFirstClient<MNFClientManagerSession>();
            if(null != session)
                session.AsyncSend((int)MNFClientManagerBasicMessageDefine.CS.CS_JoystickPosition, varServer);
        }
    }
       
    void OnDestroy()
    {
        Release();
    }

    void OnApplicationQuit()
    {
        Release();
    }

    void Release()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        TcpHelper.Instance.Stop();
        LogManager.Instance.Release();
    }
}
