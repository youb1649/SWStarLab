using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using MNF;

[System.Serializable]
public class GeoINTServerLogEvent : UnityEvent<string>
{
}

public class MNFServerManager : MonoBehaviour
{
    public GeoINTServerLogEvent thisEvent = new GeoINTServerLogEvent();

    void InsertLog(string szMsg)
    {

    }

    void Awake()
    {
        thisEvent.AddListener(InsertLog);

        LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
        {
            Debug.Log("LogWriter init failed");
            //WriteLog("LogWriter init failed");
        }

        if (TcpHelper.Instance.Start(isRunThread: false) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
            //WriteLog("TcpHelper.Instance.run() failed");
            return;
        }

        if (TcpHelper.Instance.StartAccept<MNFServerManagerSession, MNFServerManagerMessageDispatcher>("10001", 500) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<MNFServerManagerSession, MNFServerManagerMessageDispatcher>() failed");
            //WriteLog("TcpHelper.Instance.StartAccept<MNFServerManagerSession, MNFServerManagerMessageDispatcher>() failed");
            return;
        }
    }
        
    void Start()
    {
    }
     
    void Update()
    {
        TcpHelper.Instance.DipatchNetworkInterMessage();
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

    void WriteLog(string szMsg)
    {
        if (null != thisEvent)
            thisEvent.Invoke(szMsg);
    }
}
