using System.Collections;
using UnityEngine;
using UniVRM10;

public class LipSyncTest : MonoBehaviour, IMorphSource
{
	private AudioInputManager audioInputManager;
	private FaceFilter faceFilter = new FaceFilter();
	private int viseme = -1;
	private short[] data;

	IEnumerator Start()
	{
		if(TryGetComponent(out Vrm10Instance vrm))
			vrm.UpdateType = Vrm10Instance.UpdateTypes.None;
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
		PhonemeLibrary.Lipsync_Init();
		audioInputManager = GetComponent<AudioInputManager>();
		audioInputManager.OnAudio += (short[] data)=>{
			// this.data = data;
			PhonemeLibrary.Lipsync_GetVisemes(data, data.Length, ref viseme);
		};
	}
	public float[] GetMorphs()
	{
		if(TryGetComponent(out Vrm10Instance vrm))
		{
			vrm.Runtime.Process();
		}
		
		return faceFilter.Run(viseme);
	}
	// public void OnGUI()
	// {
	// 	GUIStyle style = new GUIStyle(GUI.skin.label){fontSize = 80};
	// 	style.normal.textColor = Color.green;
	// 	GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
	// 	if(data!=null)
	// 		GUILayout.Label(data[0].ToString(), style);
	// 	GUILayout.Label(viseme.ToString(), style);
	// 	GUILayout.EndVertical();
	// }
}
