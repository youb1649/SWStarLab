using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPResponse : MonoBehaviour {

    public GameObject UDPCommGameObject;
    public TextMesh textMesh = null;

	void Start () {
	
	}

	void Update () {
	
	}

	public void ResponseToUDPPacket(string fromIP, string fromPort, byte[] data)
	{
		string dataString = System.Text.Encoding.UTF8.GetString (data);

		if (textMesh != null) {
			textMesh.text = dataString;
        }

        // UTF-8 is real
        var dataBytes = System.Text.Encoding.UTF8.GetBytes(dataString);
        UDPCommunication comm = UDPCommGameObject.GetComponent<UDPCommunication>();

        // #if is required because SendUDPMessage() is async
#if !UNITY_EDITOR
			comm.SendUDPMessage(comm.externalIP, comm.externalPort, dataBytes);
#endif
    }
}