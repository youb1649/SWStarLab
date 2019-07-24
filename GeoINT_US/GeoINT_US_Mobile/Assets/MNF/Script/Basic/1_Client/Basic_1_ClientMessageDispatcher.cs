using UnityEngine.UI;
using UnityEngine;
using MNF;

/**
 * @brief Process the received message.
 * @details
 * Basic_1_ClientMessageDispatcher is a class that handles messages received by Basic_1_ClientSession.
 * Basic_1_ClientMessageDispatcher inherits the DefaultDispatchHelper class. 
 * The DefaultDispatchHelper class takes three template arguments.
 * @param Basic_1_ClientSession The first is the object to receive the message.
 * @param BasicMessageDefine The second is a class that contains a message class.
 * @param BasicMessageDefine.SC The third is the enum in which the message is defined.
 */
public class Basic_1_ClientMessageDispatcher
    : DefaultDispatchHelper<Basic_1_ClientSession, BasicMessageDefine, BasicMessageDefine.SC>
{
	int onHi_Client(Basic_1_ClientSession session, object message)
	{
		var hiClient = (BasicMessageDefine.PACK_Hi_Client)message;

        var outputField = GameObject.FindWithTag("Output").GetComponent<InputField>();
        outputField.text = hiClient.msg;
		Debug.Log(hiClient.msg);
		
        return 0;
	}

	int onHello_Client(Basic_1_ClientSession session, object message)
	{
        var helloClient = (BasicMessageDefine.PACK_Hello_Client)message;

		var outputField = GameObject.FindWithTag("Output").GetComponent<InputField>();
		outputField.text = helloClient.msg;
        Debug.Log(helloClient.msg);

		return 0;
	}
}