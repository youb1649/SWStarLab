using UnityEngine;

public class CubeMove : MonoBehaviour
{
	void Update ()
    {
        transform.position = new Vector3(Mathf.PingPong(Time.time, 4) - 1, Mathf.PingPong(Time.time, 4) - 1, transform.position.z);
	}
}
