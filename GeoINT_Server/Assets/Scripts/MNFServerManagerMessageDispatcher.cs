using UnityEngine.UI;
using UnityEngine;
using MNF;

public class MNFServerManagerMessageDispatcher 
    : DefaultDispatchHelper<MNFServerManagerSession, MNFServerManagerBasicMessageDefine, MNFServerManagerBasicMessageDefine.CS>
{
    int onHi_Server(MNFServerManagerSession session, object message)
    {
        var hiServer = (MNFServerManagerBasicMessageDefine.PACK_Hi_Server)message;

        MNFServerLogManager logboard = GameObject.FindWithTag("LogBoard").GetComponent<MNFServerLogManager>();
        logboard.InsertLog(hiServer.msg);
        Debug.Log(hiServer.msg);

        var hiClient = new MNFServerManagerBasicMessageDefine.PACK_Hi_Client();
        hiClient.msg = "Hi, Client. I'm Server.";
        session.AsyncSend((int)MNFServerManagerBasicMessageDefine.CS.Hi_Server, hiClient);

        return 0;
    }

    int onHello_Server(MNFServerManagerSession session, object message)
    {
        var helloServer = (MNFServerManagerBasicMessageDefine.PACK_Hello_Server)message;

        MNFServerLogManager logboard = GameObject.FindWithTag("LogBoard").GetComponent<MNFServerLogManager>();
        logboard.InsertLog(helloServer.msg);
        Debug.Log(helloServer.msg);

        var helloClient = new MNFServerManagerBasicMessageDefine.PACK_Hello_Client();
        helloClient.msg = "Hello, Client. I'm Server.";
        session.AsyncSend((int)MNFServerManagerBasicMessageDefine.CS.Hi_Server, helloClient);

        return 0;
    }
}
