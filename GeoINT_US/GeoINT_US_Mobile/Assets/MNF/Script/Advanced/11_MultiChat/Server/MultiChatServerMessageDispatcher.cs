using MNF_Common;
using MNF;

public class MultiChatServerMessageDispatcher : DefaultDispatchHelper<MultiChatServerSession, MultiChatMessageDefine, MultiChatMessageDefine.MESSAGES>
{
    int onCHAT(MultiChatServerSession session, object message)
    {
        var chaMessage = message as MultiChatMessageDefine.PACK_CHAT;

        // broadcast to client
        var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
        while (clientSessionEnumerator.MoveNext())
        {
            clientSessionEnumerator.Current.Value.AsyncSend(
                (int)MultiChatMessageDefine.MESSAGES.CHAT, chaMessage);
        }

        // broadcast to server
        IPPulse.Instance.SendMessage((int)MultiChatMessageDefine.MESSAGES.CHAT, chaMessage);

        return 0;
    }
}