using UnityEngine.UI;
using UnityEngine;
using MNF;

/**
 * @brief Process the received message.
 * @details
 * Basic_1_ServerMessageDispatcher is a class that handles messages received by Basic_1_ClientSession.
 * Basic_1_ServerMessageDispatcher inherits the DefaultDispatchHelper class. 
 * The DefaultDispatchHelper class takes three template arguments.
 * @param Basic_1_ServerSession The first is the object to receive the message.
 * @param BasicMessageDefine The second is a class that contains a message class.
 * @param BasicMessageDefine.CS The third is the enum in which the message is defined.
 */
public class Basic_1_ServerMessageDispatcher
    : DefaultDispatchHelper<Basic_1_ServerSession, BasicMessageDefine, BasicMessageDefine.CS>
{
	int onHi_Server(Basic_1_ServerSession session, object message)
	{
		var hiServer = (BasicMessageDefine.PACK_Hi_Server)message;
        var outputField = GameObject.FindWithTag("Output").GetComponent<InputField>();
		outputField.text = hiServer.msg;
		Debug.Log(hiServer.msg);

		var hiClient = new BasicMessageDefine.PACK_Hi_Client();
		hiClient.msg = "Hi, Client. I'm Server.";
		session.AsyncSend((int)BasicMessageDefine.CS.Hi_Server, hiClient);

		return 0;
	}

	int onHello_Server(Basic_1_ServerSession session, object message)
	{
		var helloServer = (BasicMessageDefine.PACK_Hello_Server)message;
		var outputField = GameObject.FindWithTag("Output").GetComponent<InputField>();
		outputField.text = helloServer.msg;
		Debug.Log(helloServer.msg);

		var helloClient = new BasicMessageDefine.PACK_Hello_Client();
		helloClient.msg = "Hello, Client. I'm Server.";
        session.AsyncSend((int)BasicMessageDefine.CS.Hi_Server, helloClient);

		return 0;
	}
}