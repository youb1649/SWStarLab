using UnityEngine.UI;
using UnityEngine;
using MNF;

public class InputFieldServerPort : MonoBehaviour
{
    void Start()
    {
        var input = gameObject.GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitPort);
        input.onEndEdit = se;
    }

    private void SubmitPort(string port)
    {
        LogManager.Instance.Write(port);
    }
}
