using UnityEngine.UI;
using UnityEngine;
using MNF;

/**
 * @brief The class to be connected to the Unity GameObject.
 * @details Create an MNF client and connect to the server.
 */
public class Basic_1_Client : MonoBehaviour
{
    /**
     * @brief The function that is called first when the Unity app is executed.
     * @details Set the LogManager and TcpHelper, the core of MNF.
     */
	void Awake()
    {
		// LogManager is the log interface used by MNF.
		// The LogManager will log the logs through the UnityLogWriter class.
		LogManager.Instance.SetLogWriter(new UnityLogWriter());
		if (LogManager.Instance.Init() == false)
			Debug.Log("LogWriter init failed");

		// Really important !! When starting TcpHelper in Unity, isRunThread should be set to false.
		// Otherwise, your app can crash with multi-thread issues.
		if (TcpHelper.Instance.Start(isRunThread: false) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
			return;
		}

		// When we pass Basic_1_ClientSession and Basic_1_ClientMessageDispatcher
        // as template parameter of TcpHelper.AsyncConnect function, MNF automatically
        // creates Basic_1_ClientSession object and connects to the server activated with IP: "127.0.0.1", Port: "10000".
		if (TcpHelper.Instance.AsyncConnect<Basic_1_ClientSession, Basic_1_ClientMessageDispatcher>("127.0.0.1", "10000") == null)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.AsyncConnect() failed");
			return;
		}
	}

	/**
     * @brief A function that is called every frame in Unity.
     * @details Basic_1_ClientSession can handle messages received by calling TcpHelper.Instance.dipatchNetworkInterMessage ().
     */
	void Update()
	{
		// The message received by Basic_1_ClientSession is managed as a queue inside the MNF,
        // and it is necessary to call the dipatchNetworkInterMessage () function to process the message loaded in the queue.
		TcpHelper.Instance.DipatchNetworkInterMessage();
	}

	public void OnHiServerSend()
	{
        var hiServer = new MNFServerManagerBasicMessageDefine.PACK_Hi_Server();
        hiServer.msg = "Hi, Server. I'm Client.";
        var session = TcpHelper.Instance.GetFirstClient<Basic_1_ClientSession>();
        session.AsyncSend((int)MNFServerManagerBasicMessageDefine.CS.Hi_Server, hiServer);
	}

	public void OnHelloServerSend()
	{
		var helloServer = new MNFServerManagerBasicMessageDefine.PACK_Hello_Server();
		helloServer.msg = "Hello, Server. I'm Client.";
		var session = TcpHelper.Instance.GetFirstClient<Basic_1_ClientSession>();
		session.AsyncSend((int)MNFServerManagerBasicMessageDefine.CS.Hello_Server, helloServer);
	}

    void OnDestroy()
    {
        Release();
    }

	void OnApplicationQuit()
	{
        Release();
	}

	/**
     * @brief Called when the Basic_1_Client object is released from Unity.
     * @details MNF's LogManager and TcpHelper are also released here.
     */
	void Release()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}
}
