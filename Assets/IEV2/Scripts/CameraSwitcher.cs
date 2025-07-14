using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CameraSwitcher : MonoBehaviour
{
    [Tooltip("主頁面 & 校準階段用的 Camera (Cam1)")]
    public Camera cam1;
    [Tooltip("運動準備到運動結束報告階段用的 Camera (Cam2)")]
    public Camera cam2;

    [Header("教練位置管理")]
    public CoachPositionManager coachPositionManager;  // 指向場景中同一物件

    [Header("頭部朝向控制")]
    [Tooltip("控制頭部朝向的 Multi-Aim Constraint 組件")]
    public MultiAimConstraint headAimConstraint;
    [Tooltip("Rig Builder (負責重建所有 Rig)")]
    public RigBuilder rigBuilder;
    [Tooltip("櫃台相機的索引 (通常為第一個 Source Object, 索引為 0)")]
    public int counterCameraIndex = 0;
    [Tooltip("運動空間相機的索引 (通常為第二個 Source Object, 索引為 1)")]
    public int exerciseCameraIndex = 1;

    private void Awake()
    {
        // 一開場就啟用 Cam1，並把教練瞬移到櫃台
        SwitchToCam1();
    }

    public void SwitchToCam1()
    {
        if (cam1 != null) cam1.enabled = true;
        if (cam2 != null) cam2.enabled = false;

        coachPositionManager?.TeleportToCounter();
        SetHeadAimWeights(1f, 0f);

        Debug.Log("[CameraSwitcher] 切換到 Cam1");
    }

    public void SwitchToCam2()
    {
        if (cam1 != null) cam1.enabled = false;
        if (cam2 != null) cam2.enabled = true;

        coachPositionManager?.TeleportToExercise();
        SetHeadAimWeights(0f, 1f);

        Debug.Log("[CameraSwitcher] 切換到 Cam2");
    }

    private void SetHeadAimWeights(float counterWeight, float exerciseWeight)
    {
        if (headAimConstraint == null) return;

        // 取得並修改 data
        var data = headAimConstraint.data;
        var sourceObjects = data.sourceObjects;

        if (counterCameraIndex >= 0 && counterCameraIndex < sourceObjects.Count)
        {
            var src = sourceObjects[counterCameraIndex];
            src.weight = counterWeight;
            sourceObjects[counterCameraIndex] = src;
        }

        if (exerciseCameraIndex >= 0 && exerciseCameraIndex < sourceObjects.Count)
        {
            var src = sourceObjects[exerciseCameraIndex];
            src.weight = exerciseWeight;
            sourceObjects[exerciseCameraIndex] = src;
        }

        data.sourceObjects = sourceObjects;
        headAimConstraint.data = data;

        // 強制刷新 Constraint 權重
        headAimConstraint.weight = 0f;
        headAimConstraint.weight = 1f;

        // 如有 RigBuilder，強制重建全部 Rig
        rigBuilder?.Build();
    }
}
