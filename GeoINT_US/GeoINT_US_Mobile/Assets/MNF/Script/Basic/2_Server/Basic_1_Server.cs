using UnityEngine;
using UnityEngine.UI;
using MNF;

/**
 * @brief The class to be connected to the Unity GameObject.
 * @details Create an MNF server and listen, bind, accept for client.
 */
public class Basic_1_Server : MonoBehaviour
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

		// If Basic_1_ServerSession and Basic_1_ServerMessageDispatcher are passed as template parameters of the TcpHelper.StartAccept function, 
		// MNF will automatically create a Basic_1_ServerSession object when Basic_1_ClientSession contacts the server.
		// The Basic_1_Server server should be connected with IP: "127.0.0.1" and Port: "10000".
		if (TcpHelper.Instance.StartAccept<Basic_1_ServerSession, Basic_1_ServerMessageDispatcher>("10000", 500) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<Basic_1_ServerSession, Basic_1_ServerMessageDispatcher>() failed");
			return;
		}
	}

	/**
     * @brief A function that is called every frame in Unity.
     * @details Basic_1_ServerSession can handle messages received by calling TcpHelper.Instance.dipatchNetworkInterMessage ().
     */
	void Update()
	{
		// The message received by Basic_1_ServerSession is managed as a queue inside the MNF,
		// and it is necessary to call the dipatchNetworkInterMessage () function to process the message loaded in the queue.
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

	/**
     * @brief Called when the Basic_1_Server object is released from Unity.
     * @details MNF's LogManager and TcpHelper are also released here.
     */
	void Release()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}
}
