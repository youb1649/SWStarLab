using MNF_Common;
using MNF;

public class ChatServerMessageDispatcher : DefaultDispatchHelper<ChatServerSession, BinaryChatMessageDefine, BinaryChatMessageDefine.ENUM_CS_>
{
    int onCS_SEND_CHAT_MESSAGE(ChatServerSession session, object message)
    {
        var chaMessage = message as BinaryChatMessageDefine.PACK_CS_SEND_CHAT_MESSAGE;
        MNF_ChatServer.Instance.BinaryChatServerScenePoint.recvMessageBox.text += chaMessage.stringField + "\n";

        return 0;
    }

    int onCS_BROADCAST_CHAT_MESSAGE(ChatServerSession session, object message)
    {
        var chaMessage = message as BinaryChatMessageDefine.PACK_CS_BROADCAST_CHAT_MESSAGE;
        var broadcastChatMessage = new BinaryChatMessageDefine.PACK_SC_BROADCAST_CHAT_MESSAGE();
        broadcastChatMessage.stringField = chaMessage.stringField;

        MNF_ChatServer.Instance.BinaryChatServerScenePoint.recvMessageBox.text += chaMessage.stringField + "\n";

        var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
        while (clientSessionEnumerator.MoveNext())
        {
            clientSessionEnumerator.Current.Value.AsyncSend(
                (int)BinaryChatMessageDefine.ENUM_SC_.SC_BROADCAST_CHAT_MESSAGE, broadcastChatMessage);
        }

        return 0;
    }
}