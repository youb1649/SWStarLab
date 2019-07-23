using UnityEngine;
using UnityEngine.SceneManagement;
using MNF_Common;
using MNF;

public class ChatButtonTrigger : MonoBehaviour
{

    public void OnServer()
    {
        SceneManager.LoadScene("3_BinaryChatServer");
    }

    public void OnClient()
    {
        SceneManager.LoadScene("3_BinaryChatClient");
    }

    #region SERVER
    public void OnStartBinaryChatServer()
    {
        LogManager.Instance.Write("Start Binary Chat Server");

		if (MNF_ChatServer.Instance.init() == true)
            LogManager.Instance.Write("MNF_ChatServer init() success");
        else
            LogManager.Instance.Write("MNF_ChatServer init() failed");
    }

    public void OnBroadcastBinaryChatMessage_Server()
    {
        if (MNF_ChatServer.Instance.IsInit == false)
            return;

        if (TcpHelper.Instance.GetClientSessionCount() == 0)
            return;

        var InputText = MNF_ChatServer.Instance.BinaryChatServerScenePoint.inputNicknameBox.text + " : ";
        InputText += MNF_ChatServer.Instance.BinaryChatServerScenePoint.inputChatMessageBox.text;
        LogManager.Instance.Write("Send Binary Chat Message : {0}", InputText);

        var message = new BinaryChatMessageDefine.PACK_SC_BROADCAST_CHAT_MESSAGE();
        message.stringField = InputText;

        var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
        while (clientSessionEnumerator.MoveNext())
        {
            clientSessionEnumerator.Current.Value.AsyncSend(
                (int)BinaryChatMessageDefine.ENUM_SC_.SC_BROADCAST_CHAT_MESSAGE, message);
        }

        MNF_ChatServer.Instance.BinaryChatServerScenePoint.recvMessageBox.text += InputText + "\n";
        MNF_ChatServer.Instance.BinaryChatServerScenePoint.inputChatMessageBox.text = "";
    }

    public void OnSingleBinaryChatMessage_Server()
    {
        if (MNF_ChatServer.Instance.IsInit == false)
            return;

        if (TcpHelper.Instance.GetClientSessionCount() == 0)
            return;

        var dropdown = MNF_ChatServer.Instance.BinaryChatServerScenePoint.dropdownSessins;
        ChatServerSession foundSession = null;

        var clientSessionEnumerator = TcpHelper.Instance.GetClientSessionEnumerator();
        while(clientSessionEnumerator.MoveNext())
        {
            var session = (ChatServerSession)clientSessionEnumerator.Current.Value;
            if (session.GetSessionCode() == dropdown.options[dropdown.value].text)
            {
                foundSession = session as ChatServerSession;
                break;
            }
        }

        var InputText = MNF_ChatServer.Instance.BinaryChatServerScenePoint.inputNicknameBox.text + " : ";
        InputText += MNF_ChatServer.Instance.BinaryChatServerScenePoint.inputChatMessageBox.text;
        LogManager.Instance.Write("Send Binary Chat Message : {0}", InputText);

        var message = new BinaryChatMessageDefine.PACK_SC_BROADCAST_CHAT_MESSAGE();
        message.stringField = InputText;
 
        foundSession.AsyncSend((int)BinaryChatMessageDefine.ENUM_SC_.SC_BROADCAST_CHAT_MESSAGE, message);

        MNF_ChatServer.Instance.BinaryChatServerScenePoint.recvMessageBox.text += InputText + "\n";
        MNF_ChatServer.Instance.BinaryChatServerScenePoint.inputChatMessageBox.text = "";
    }

    public void OnClearChatMessage_Server()
    {
        MNF_ChatServer.Instance.BinaryChatServerScenePoint.recvMessageBox.text = "";
    }
    #endregion

    #region CLIENT
    public void OnConnectToBinaryChatServer()
    {
        LogManager.Instance.Write("Start Binary Chat Client");

        if (MNF_ChatClient.Instance.Init() == true)
            LogManager.Instance.Write("MNF_ChatClient init() success");
        else
            LogManager.Instance.Write("MNF_ChatClient init() failed");
    }

    public void OnSendBinaryChatMessage()
    {
        if (MNF_ChatClient.Instance.IsInit == false)
            return;

        var InputText = MNF_ChatClient.Instance.BinaryChatClientScenePoint.inputNicknameBox.text + " : ";
        InputText += MNF_ChatClient.Instance.BinaryChatClientScenePoint.inputChatMessageBox.text;
        LogManager.Instance.Write("Send Binary Chat Message : {0}", InputText);

        var message = new BinaryChatMessageDefine.PACK_CS_SEND_CHAT_MESSAGE();
        message.stringField = InputText;
        var clientSession = TcpHelper.Instance.GetFirstClient<ChatClientSession>();
        clientSession.AsyncSend((int)BinaryChatMessageDefine.ENUM_CS_.CS_SEND_CHAT_MESSAGE, message);

        MNF_ChatClient.Instance.BinaryChatClientScenePoint.recvMessageBox.text += InputText + "\n";
        MNF_ChatClient.Instance.BinaryChatClientScenePoint.inputChatMessageBox.text = "";
    }

    public void OnBroadcastBinaryChatMessage_Client()
    {
        if (MNF_ChatClient.Instance.IsInit == false)
            return;

        var InputText = MNF_ChatClient.Instance.BinaryChatClientScenePoint.inputNicknameBox.text + " : ";
        InputText += MNF_ChatClient.Instance.BinaryChatClientScenePoint.inputChatMessageBox.text;
        LogManager.Instance.Write("Broadcast Binary Chat Message : {0}", InputText);

        var message = new BinaryChatMessageDefine.PACK_CS_BROADCAST_CHAT_MESSAGE();
        message.stringField = InputText;
        var clientSession = TcpHelper.Instance.GetFirstClient<ChatClientSession>();
        clientSession.AsyncSend((int)BinaryChatMessageDefine.ENUM_CS_.CS_BROADCAST_CHAT_MESSAGE, message);

        MNF_ChatClient.Instance.BinaryChatClientScenePoint.inputChatMessageBox.text = "";
    }

    public void OnClearChatMessage_Client()
    {
        MNF_ChatClient.Instance.BinaryChatClientScenePoint.recvMessageBox.text = "";
    }
    #endregion
}
