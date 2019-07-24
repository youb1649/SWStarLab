using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MNF;

public class EchoButtonTrigger : MonoBehaviour
{
    public EchoServerScene EchoServerScenePoint { get; set; }

    public void OnEchoServer()
    {
        SceneManager.LoadScene("2_EchoServerScene");
    }
    public void OnEchoClient()
    {
        SceneManager.LoadScene("2_EchoClientScene");
    }

    public void OnStartBinaryJsonEchoClient()
    {
        LogManager.Instance.Write("Start Binary/Json Echo Client");

        if (MNF_EchoClient.Instance.OnStart() == true)
            LogManager.Instance.Write("MNF_EchoClient.Instance.init() success");
        else
            LogManager.Instance.Write("MNF_EchoClient.Instance.init() failed");
    }

    public void OnStartBinaryJsonEchoServer()
    {
        LogManager.Instance.Write("Start Binary/Json Echo Server");

        if (MNF_EchoServer.Instance.Init() == true)
            LogManager.Instance.Write("MNF_EchoServer.Instance.init() success");
        else
            LogManager.Instance.Write("MNF_EchoServer.Instance.init() failed");
    }
}
