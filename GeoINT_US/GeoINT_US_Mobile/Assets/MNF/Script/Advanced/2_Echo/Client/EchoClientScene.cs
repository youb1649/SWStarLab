using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class EchoClientScene : IScene
{
    public InputField ipList;
    public InputField serverIP;
    public InputField binaryServerPort;
    public InputField jsonServerPort;
    public Text outputField_Binary;
    public Text outputField_Json;

    public MessageDispatchInfo binaryDispatch = new MessageDispatchInfo();
    public MessageDispatchInfo jsonDispatch = new MessageDispatchInfo();

    public float elapsedTime = 0.0f;
    public float totalelapsedTime = 0.0f;

    public EchoClientScene()
    {
        SceneName = "EchoClientScene";
    }

    void Awake()
    {
        LogManager.Instance.SetLogWriter(new UnityLogWriter());
        if (LogManager.Instance.Init() == false)
            Debug.Log("LogWriter init failed");

        MNF_EchoClient.Instance.EchoClientScenePoint = this;

        List<string> getPrivateIPList;
        List<string> getPrivateIPDescList;
        Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);
        ipList.text = string.Join("\n", getPrivateIPList);

        if (IPPulse.Instance.Start(getPrivateIPList, "999", true) == false)
            LogManager.Instance.WriteError("LookAround start failed");
    }

    void Start()
    {
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        totalelapsedTime += Time.deltaTime;

        if (elapsedTime >= 1.0f)
        {
            outputField_Binary.text = getBinaryProfiling();
            outputField_Json.text = getJsonProfiling();

            elapsedTime = 0.0f;
        }
    }

    public void updateBinaryProfiling(int echoPacketSize)
    {
        ++binaryDispatch.dispatchCount;
        ++binaryDispatch.totalDispatchCount;

        binaryDispatch.dispatchSize += echoPacketSize;
        binaryDispatch.totalDispatchSize += echoPacketSize;
    }

    public string getBinaryProfiling()
    {
        string profilingString = string.Format("Per count : {0}\nPer bytes : {1}(kb)\nTotal count : {2}\nTotal size : {3}(kb)\n Total elapsed : {4}",
                binaryDispatch.dispatchCount / elapsedTime, binaryDispatch.dispatchSize / 1024, binaryDispatch.totalDispatchCount, binaryDispatch.totalDispatchSize / 1024, totalelapsedTime);

        binaryDispatch.init();

        return profilingString;
    }

    public void updateJsonProfiling()
    {
        ++jsonDispatch.dispatchCount;
        ++jsonDispatch.totalDispatchCount;
    }

    public string getJsonProfiling()
    {
        string profilingString = string.Format("Per count : {0}\nTotal count : {1}\nTotal elapsed : {2}",
                jsonDispatch.dispatchCount / elapsedTime, jsonDispatch.totalDispatchCount, totalelapsedTime);

        jsonDispatch.init();

        return profilingString;
    }
}