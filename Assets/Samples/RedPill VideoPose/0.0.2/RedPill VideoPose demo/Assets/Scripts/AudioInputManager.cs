using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AudioInputManager : MonoBehaviour
{
	public int id=0;
	public delegate void OnAudioDelegate(short[] data);
	public event OnAudioDelegate OnAudio;

	IEnumerator Start()
	{
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
		MiniAudio.SetAudioCallback(OnAudioCallback);
		MiniAudio.StartMicrophone(id, 16000, 1);
	}
	private void OnAudioCallback(short[] data)
	{
		OnAudio?.Invoke(data);
	}

	void OnDestroy()
	{
		MiniAudio.StopMicrophone();
	}
}
