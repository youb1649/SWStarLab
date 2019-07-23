using UnityEngine.UI;
using UnityEngine;
using MNF;

public class InputNicknameBox : MonoBehaviour
{
    void Start()
    {
        var input = gameObject.GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitNickname);
        input.onEndEdit = se;
    }

    private void SubmitNickname(string nickname)
    {
        LogManager.Instance.Write("Set Nickname : " + nickname);
    }
}
