using MNF;
using UnityEngine;

public class MultiChatServerSession : JsonSession
{
    public string GetSessionCode()
    {
        return string.Format("{0}:{1}", EndPoint, ToString());
    }

    public override int OnAccept()
    {
        TcpHelper.Instance.AddClientSession(GetHashCode(), this);
        LogManager.Instance.Write("OnAccept : {0}={1}={2}={3}", ToString(), GetType(), EndPoint, GetSessionCode());

        var foundButtonTrigger = MultiChatServerButtonTrigger.FindMultiChatServerButtonTrigger();

        var mnfSessionData = new UnityEngine.UI.Dropdown.OptionData(GetSessionCode());
        foundButtonTrigger.clientDropdown.options.Add(mnfSessionData);

        if (foundButtonTrigger.clientDropdown.options.Count == 1)
            foundButtonTrigger.clientDropdown.captionText.text = mnfSessionData.text;

        return 0;
    }

    public override int OnDisconnect()
    {
        LogManager.Instance.Write("onDisconnect : {0}={1}={2}={3}", ToString(), GetType(), EndPoint, GetSessionCode());
        TcpHelper.Instance.RemoveClientSession(this);

        var foundButtonTrigger = MultiChatServerButtonTrigger.FindMultiChatServerButtonTrigger();
        var dropdown = foundButtonTrigger.clientDropdown;
        foreach (var option in dropdown.options)
        {
            if (option.text != GetSessionCode())
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