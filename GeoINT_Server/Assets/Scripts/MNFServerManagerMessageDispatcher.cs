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
        if (null != logboard)
            logboard.InsertLog(varServer.msg);

        UDPManager udpmanager = GameObject.FindWithTag("UdpManager").GetComponent<UDPManager>();
        if (null != udpmanager)
            udpmanager.SendMsg(varServer.msg);

        return 0;
    }
}
