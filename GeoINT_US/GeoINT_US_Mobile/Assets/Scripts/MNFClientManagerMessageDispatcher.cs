using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class MNFClientManagerMessageDispatcher
    : DefaultDispatchHelper<MNFClientManagerSession, MNFClientManagerBasicMessageDefine, MNFClientManagerBasicMessageDefine.SC>
{
    /*
    int onHi_Client(MNFClientManagerSession session, object message)
    {
        var hiClient = (MNFClientManagerBasicMessageDefine.PACK_Hi_Client)message;

        var outputField = GameObject.FindWithTag("Output").GetComponent<InputField>();
        outputField.text = hiClient.msg;
        Debug.Log(hiClient.msg);

        return 0;
    }

    int onHello_Client(MNFClientManagerSession session, object message)
    {
        var helloClient = (MNFClientManagerBasicMessageDefine.PACK_Hello_Client)message;

        var outputField = GameObject.FindWithTag("Output").GetComponent<InputField>();
        outputField.text = helloClient.msg;
        Debug.Log(helloClient.msg);

        return 0;
    }
    */
}
