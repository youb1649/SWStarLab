using MNF;

public class ChatServerSession : BinarySession
{
    public string GetSessionCode()
    {
        return string.Format("{0}:{1}", EndPoint, ToString());
    }

    public override int OnAccept()
    {
        LogManager.Instance.Write("OnAccept : {0}:{1}:{2}", ToString(), GetType(), EndPoint);

        TcpHelper.Instance.AddClientSession(GetHashCode(), this);

        var mnfSessionData = new UnityEngine.UI.Dropdown.OptionData(GetSessionCode());
        MNF_ChatServer.Instance.BinaryChatServerScenePoint.dropdownSessins.options.Add(mnfSessionData);

        if (MNF_ChatServer.Instance.BinaryChatServerScenePoint.dropdownSessins.options.Count == 1)
            MNF_ChatServer.Instance.BinaryChatServerScenePoint.dropdownSessins.captionText.text = mnfSessionData.text;

        return 0;
    }

    public override int OnDisconnect()
    {
        LogManager.Instance.Write("onDisconnect : {0}:{1}:{2}", ToString(), GetType(), EndPoint);

        TcpHelper.Instance.RemoveClientSession(this);

        var dropdown = MNF_ChatServer.Instance.BinaryChatServerScenePoint.dropdownSessins;
        foreach (var option in dropdown.options)
        {
            if (option.text != ToString())
                continue;

            dropdown.options.Remove(option);

            if (dropdown.options.Count == 0)
            {
                dropdown.captionText.text = "";
            }
            else if (dropdown.captionText.text == GetSessionCode())
            {
                dropdown.captionText.text = dropdown.options[0].text;
            }
            break;
        }

        return 0;
    }
}