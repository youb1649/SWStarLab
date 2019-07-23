using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiChatStarterButtonTrigger : MonoBehaviour
{
    const int ServerToServerPort = 50000;
    int ClientToServerPort = 0;

    // chat server 1
    public void OnServer_1()
    {
        ClientToServerPort = 10000;
        PlayerPrefs.SetInt("ClientToServerPort", ClientToServerPort);
        PlayerPrefs.SetInt("ServerToServerPort", ServerToServerPort);
        PlayerPrefs.SetString("ServerName", "Server_1");

        SceneManager.LoadScene("11_ChatServer");
    }

    public void OnClient_1_1()
    {
        ClientToServerPort = 10000;
        PlayerPrefs.SetInt("ClientToServerPort", ClientToServerPort);
        PlayerPrefs.SetInt("ServerToServerPort", ServerToServerPort);
        PlayerPrefs.SetString("ClientName", "Client_1_1");

        SceneManager.LoadScene("11_ChatClient");
    }

    public void OnClient_1_2()
    {
        ClientToServerPort = 10000;
        PlayerPrefs.SetInt("ClientToServerPort", ClientToServerPort);
        PlayerPrefs.SetInt("ServerToServerPort", ServerToServerPort);
        PlayerPrefs.SetString("ClientName", "Client_1_2");

        SceneManager.LoadScene("11_ChatClient");
    }

    // chat server 2
    public void OnServer_2()
    {
        ClientToServerPort = 20000;
        PlayerPrefs.SetInt("ClientToServerPort", ClientToServerPort);
        PlayerPrefs.SetInt("ServerToServerPort", ServerToServerPort);
        PlayerPrefs.SetString("ServerName", "Server_2");

        SceneManager.LoadScene("11_ChatServer");
    }

    public void OnClient_2_1()
    {
        ClientToServerPort = 20000;
        PlayerPrefs.SetInt("ClientToServerPort", ClientToServerPort);
        PlayerPrefs.SetInt("ServerToServerPort", ServerToServerPort);
        PlayerPrefs.SetString("ClientName", "Client_2_1");

        SceneManager.LoadScene("11_ChatClient");
    }

    public void OnClient_2_2()
    {
        ClientToServerPort = 20000;
        PlayerPrefs.SetInt("ClientToServerPort", ClientToServerPort);
        PlayerPrefs.SetInt("ServerToServerPort", ServerToServerPort);
        PlayerPrefs.SetString("ClientName", "Client_2_2");

        SceneManager.LoadScene("11_ChatClient");
    }
}
