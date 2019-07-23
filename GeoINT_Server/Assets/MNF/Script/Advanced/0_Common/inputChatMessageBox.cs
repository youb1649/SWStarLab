using UnityEngine.UI;
using UnityEngine;
using MNF;

public class inputChatMessageBox : MonoBehaviour
{
    public InputField nicknameInputField = null;

    public string InputChatMessage { get; set; }

    void Awake()
    {
        InputChatMessage = "";
    }

    void Start()
    {
        var input = gameObject.GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitChat);
        input.onEndEdit = se;
    }

    private void SubmitChat(string chat)
    {
        InputChatMessage = nicknameInputField.text + " : " + chat;
    }
}
