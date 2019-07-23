using MNF_Common;
using MNF;

public class MultiChatClientMessageDispatcher : DefaultDispatchHelper<MultiChatClientSession, MultiChatMessageDefine, MultiChatMessageDefine.MESSAGES>
{
    int onCHAT(MultiChatClientSession session, object message)
    {
        var foundButtonTrigger = MultiChatClientButtonTrigger.FindMultiChatClientButtonTrigger();

        var chaMessage = message as MultiChatMessageDefine.PACK_CHAT;
        string splitedText = foundButtonTrigger.inputRecvChatMessage.text + '\n' + chaMessage.chat;
        var splitedTextList = splitedText.Split('\n');

        int maxLen = 10;

        foundButtonTrigger.inputRecvChatMessage.text = "";
        if (splitedTextList.Length <= maxLen)
        {
            for (int i = 0; i < splitedTextList.Length; ++i)
                foundButtonTrigger.inputRecvChatMessage.text = foundButtonTrigger.inputRecvChatMessage.text + '\n' + splitedTextList[i];
        }
        else
        {
            for (int i = splitedTextList.Length - maxLen; i < splitedTextList.Length; ++i)
                foundButtonTrigger.inputRecvChatMessage.text = foundButtonTrigger.inputRecvChatMessage.text + '\n' + splitedTextList[i];
        }

        return 0;
    }
}