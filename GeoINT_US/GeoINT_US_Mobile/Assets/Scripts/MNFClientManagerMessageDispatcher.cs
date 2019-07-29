using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class MNFClientManagerMessageDispatcher
    : DefaultDispatchHelper<MNFClientManagerSession, MNFClientManagerBasicMessageDefine, MNFClientManagerBasicMessageDefine.SC>
{
    int onSC_JoystickPosition(MNFClientManagerSession session, object message)
    {
        var varClient = (MNFClientManagerBasicMessageDefine.PACK_SC_JoystickPosition)message;

        //var outputField = GameObject.FindWithTag("Output").GetComponent<InputField>();
        //outputField.text = varClient.msg;
        //Debug.Log(varClient.msg);

        return 0;
    }
}
