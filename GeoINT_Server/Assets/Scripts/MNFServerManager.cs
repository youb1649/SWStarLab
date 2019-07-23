using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MNF;

public class MNFServerManager : MonoBehaviour
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

        if (TcpHelper.Instance.StartAccept<MNFServerManagerSession, MNFServerManagerMessageDispatcher>("10000", 500) == false)
        {
            LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<MNFServerManagerSession, MNFServerManagerMessageDispatcher>() failed");
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
}
