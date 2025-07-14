using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
class Poses
{
	public Pose[] list;
}

public class VideoPoseTest : MonoBehaviour, IMotionSource
{
	private Queue<Pose> queue;
	public InputManager inputManager;
	private Touch touch;
	private Pose pose;
	private VideoPose videoPose;
	public bool showGUI = false;
	public bool upperBodyMode = false;
	public bool detectAPose = false;
	public void Start()
    {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		videoPose = new VideoPose();
		bool res = videoPose.Start(OnPose);
		videoPose.Reset(detect_apose:detectAPose);
		videoPose.SetUpperBodyMode(upperBodyMode);
		if(inputManager==null)
			inputManager = GetComponent<InputManager>();
		inputManager.OnNewFrame += (ImageBuffer buffer)=>{
			videoPose.PushFrame(buffer.id, buffer.buffer, buffer.width, buffer.height, 4);
		};
		inputManager.rawImage.enabled = showGUI;
		if(TryGetComponent(out touch))
		{
			touch.tapped += ()=>{
				showGUI = !showGUI;
				inputManager.rawImage.enabled = showGUI;
			};
		}
	}
	public Pose GetPose()
	{
		return pose;
    }
	private void OnPose(Pose pose)
	{
		queue?.Enqueue(pose);
		this.pose = pose;
	}

	private void OnGUI()
	{
		// if(!showGUI)
		// 	return;
		GUIStyle style = new GUIStyle(GUI.skin.label){fontSize = 80};
		style.normal.textColor = Color.green;
		GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		if(GUILayout.Button("Start", style))
		{
			queue = new Queue<Pose>();
		}
		if(GUILayout.Button("Stop", style))
		{
			var poses = new Poses();
			poses.list = queue.ToArray();
			var j = JsonUtility.ToJson(poses);
			Debug.Log(queue.Count);
			Debug.Log(j);
			File.WriteAllText("D:/tmp/mocap.json", j);
			queue = null;
		}
		GUILayout.EndVertical();
		// if (GUI.Button(new Rect(100, 100, 500, 100), "Toggle Upper Body Mode", style))
		// 	videoPose.SetUpperBodyMode(upperBodyMode = !upperBodyMode);
	}
}
