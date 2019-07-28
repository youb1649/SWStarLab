using UnityEngine.UI;
using UnityEngine;
using MNF;

public class MNFServerManagerMessageDispatcher 
    : DefaultDispatchHelper<MNFServerManagerSession, MNFServerManagerBasicMessageDefine, MNFServerManagerBasicMessageDefine.CS>
{
    int onCS_JoystickPosition(MNFServerManagerSession session, object message)
    {
        var varServer = (MNFServerManagerBasicMessageDefine.PACK_CS_JoystickPosition)message;

        MNFServerLogManager logboard = GameObject.FindWithTag("LogBoard").GetComponent<MNFServerLogManager>();
        logboard.InsertLog(varServer.msg);
        Debug.Log(varServer.msg);

        //var hiClient = new MNFServerManagerBasicMessageDefine.PACK_CS_JoystickPosition();
        //hiClient.msg = "Hi, Client. I'm Server.";
        //session.AsyncSend((int)MNFServerManagerBasicMessageDefine.CS.CS_JoystickPosition, hiClient);
        return 0;
    }
}
