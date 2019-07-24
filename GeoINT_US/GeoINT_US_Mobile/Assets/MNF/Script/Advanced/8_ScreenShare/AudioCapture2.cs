//using UnityEngine;

//public class MicrophoneOn : MonoBehaviour
//{
//    //AudioSource audio;
//    //void Start()
//    //{
//    //    audio = GetComponent<AudioSource>();
//    //    audio.clip = Microphone.Start(null, true, 100, 44100);
//    //    audio.loop = true;
//    //    while (!(Microphone.GetPosition(null) > 0)) { }
//    //    Debug.Log("start playing... position is " + Microphone.GetPosition(null));
//    //    audio.Play();
//    //}

//    public float gain;

//    void OnAudioFilterRead(float[] data, int channels)
//	{
//		for (var i = 0; i < data.Length; ++i)
//			data[i] = data[i] * gain;
//	}
//}