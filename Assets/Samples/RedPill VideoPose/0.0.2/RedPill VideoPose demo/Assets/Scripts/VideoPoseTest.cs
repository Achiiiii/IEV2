using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Redpill VideoPoseTest 加入 A-Pose 偵測事件
/// </summary>
[RequireComponent(typeof(InputManager))]
public class VideoPoseTest : MonoBehaviour, IMotionSource
{
    private Queue<Pose> queue;
    private Pose currentPose;
    private VideoPose videoPose;

    [Header("外部設定")]
    public InputManager inputManager;
    public bool upperBodyMode = false;
    public bool detectAPose = false;

    /// <summary> 當真正偵測到一次 A-Pose 時觸發 </summary>
    public event Action APoseDetected;

    private bool hasDetectedAPose = false;
    public string myText = "";
    public bool detected = false;

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 1. 建立並啟動 VideoPose
        videoPose = new VideoPose();
        bool ok = videoPose.Start(OnPose);
        Debug.Log($"[VideoPoseTest] videoPose.Start result = {ok}");
        videoPose.Reset(detect_apose: detectAPose);
        videoPose.SetUpperBodyMode(upperBodyMode);

        // 2. 綁定 InputManager
        if (inputManager == null)
            inputManager = GetComponent<InputManager>();
        inputManager.OnNewFrame += buffer =>
            videoPose.PushFrame(buffer.id, buffer.buffer, buffer.width, buffer.height, 4);
    }

    /// <summary> 供外部讀取最新偵測到的 Pose </summary>
    public Pose GetPose() => currentPose;

    /// <summary> VideoPose 每有一筆 Pose 就呼叫這裡 </summary>
    private void OnPose(Pose pose)
    {
        currentPose = pose;
        queue?.Enqueue(pose);

        // 如果開啟 A-Pose 偵測，且還沒觸發過
        if (detectAPose && !hasDetectedAPose && detected)
        {
            hasDetectedAPose = true;
            Debug.Log("[VideoPoseTest] A-Pose detected!11111");
            // APoseDetected?.Invoke();
        }
        var poseJson = JsonUtility.ToJson(pose);

        var parsedPose = JsonUtility.FromJson<Pose>(poseJson);
        List<Vector3> globalTransformsList = new List<Vector3>();
        foreach (var globalTransform in parsedPose.globalTransforms)
        {

            Vector3 position = new Vector3(globalTransform.m03, globalTransform.m13, globalTransform.m23);
            //Debug.Log(position);
            globalTransformsList.Add(position);
        }
        /*for(int i = 0; i < globalTransformsList.Count; i++)
		{
				var globalTransform = globalTransformsList[i];
		}*/



        string tmp = string.Join(", ", globalTransformsList);



        tmp = "[" + tmp + "]";
        myText = tmp;
        if (myText != "[]" &&myText != "") {
            detected = true;
        }
    }

    /// <summary> 外部可呼叫，重置 A-Pose 偵測狀態 </summary>
    public void ResetAPoseDetection()
    {
        hasDetectedAPose = false;
        Debug.Log("[VideoPoseTest] A-Pose detection reset.");
    }
}
