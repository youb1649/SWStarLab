using UnityEngine;
using UnityEngine.SceneManagement;

public class StarterButtonTrigger : MonoBehaviour {

	public void OnBasicServer()
	{
		SceneManager.LoadScene("Basic_1_Server");
	}
	public void OnBasicClient()
	{
		SceneManager.LoadScene("Basic_1_Client");
	}
    public void OnEchoServer()
    {
        SceneManager.LoadScene("2_EchoServerScene");
    }
    public void OnEchoClient()
    {
        SceneManager.LoadScene("2_EchoClientScene");
    }
    public void OnChatServer()
    {
        SceneManager.LoadScene("3_BinaryChatServer");
    }
    public void OnChatClient()
    {
        SceneManager.LoadScene("3_BinaryChatClient");
    }
    public void OnIL2CPP()
    {
        SceneManager.LoadScene("4_IL2CPP_Test");
    }
	public void OnUDPClient()
	{
        SceneManager.LoadScene("5_UNet_LLAPI");
    }
    public void OnIPPulse()
    {
        SceneManager.LoadScene("6_IPPulse");
    }
    public void OnFileTransServer()
	{
		SceneManager.LoadScene("7_FileTransServer");
	}
	public void OnFileTransClient()
	{
		SceneManager.LoadScene("7_FileTransClient");
	}
	public void OnScreenShareServer()
	{
		SceneManager.LoadScene("8_ScreenShareServer");
	}
	public void OnScreenShareClient()
	{
		SceneManager.LoadScene("8_ScreenShareClient");
	}
	public void OnLoginServer_MySql()
	{
		SceneManager.LoadScene("9_LoginServer_Mysql");
	}
	public void OnLoginClient()
	{
		SceneManager.LoadScene("9_LoginClient");
	}
	public void OnVideoServer()
	{
		SceneManager.LoadScene("10_VideoServer");
	}
	public void OnVideoClient()
	{
		SceneManager.LoadScene("10_VideoClient");
    }
    public void OnMultiChatStarter()
    {
        SceneManager.LoadScene("11_ChatStarter");
    }
}
