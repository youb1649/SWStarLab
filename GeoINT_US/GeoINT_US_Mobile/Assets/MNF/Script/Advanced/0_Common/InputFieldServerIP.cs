using UnityEngine.UI;
using UnityEngine;
using MNF;

public class InputFieldServerIP : MonoBehaviour
{
    void Start()
    {
        var input = gameObject.GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitIP);
        input.onEndEdit = se;
    }

    void SubmitIP(string ip)
    {
        LogManager.Instance.Write(ip);
    }
}
