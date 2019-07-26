using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class MNFClientManager : MonoBehaviour
{
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

        if (TcpHelper.Instance.AsyncConnect<MNFClientManagerSession, MNFClientManagerMessageDispatcher>("127.0.0.1", "10000") == null)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect() failed");
            return;
        }
    }

    void Update()
    {
        TcpHelper.Instance.DipatchNetworkInterMessage();
    }

    public void OnHiServerSend()
    {
        var hiServer = new MNFClientManagerBasicMessageDefine.PACK_Hi_Server();
        hiServer.msg = "Hi, Server. I'm Client.";
        var session = TcpHelper.Instance.GetFirstClient<MNFClientManagerSession>();
        session.AsyncSend((int)MNFClientManagerBasicMessageDefine.CS.Hi_Server, hiServer);
    }

    public void OnHelloServerSend()
    {
        var helloServer = new MNFClientManagerBasicMessageDefine.PACK_Hello_Server();
        helloServer.msg = "Hello, Server. I'm Client.";
        var session = TcpHelper.Instance.GetFirstClient<MNFClientManagerSession>();
        session.AsyncSend((int)MNFClientManagerBasicMessageDefine.CS.Hello_Server, helloServer);
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
