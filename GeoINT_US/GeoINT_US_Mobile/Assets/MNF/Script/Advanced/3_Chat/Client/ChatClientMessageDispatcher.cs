using MNF_Common;
using MNF;

public class ChatClientMessageDispatcher : DefaultDispatchHelper<ChatClientSession, BinaryChatMessageDefine, BinaryChatMessageDefine.ENUM_SC_>
{
    int onSC_SEND_CHAT_MESSAGE(ChatClientSession session, object message)
    {
        var chaMessage = message as BinaryChatMessageDefine.PACK_SC_SEND_CHAT_MESSAGE;
        MNF_ChatClient.Instance.BinaryChatClientScenePoint.recvMessageBox.text += chaMessage.stringField + "\n";
        return 0;
    }

    int onSC_BROADCAST_CHAT_MESSAGE(ChatClientSession session, object message)
    {
        var chaMessage = message as BinaryChatMessageDefine.PACK_SC_BROADCAST_CHAT_MESSAGE;
        MNF_ChatClient.Instance.BinaryChatClientScenePoint.recvMessageBox.text += chaMessage.stringField + "\n";
        return 0;
    }
}