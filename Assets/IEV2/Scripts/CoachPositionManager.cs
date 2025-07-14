using System.Collections;
using UnityEngine;

public class CoachPositionManager : MonoBehaviour
{
    [Header("教練模型 Transform")]
    public Transform coachTransform;

    [Header("位置錨點")]
    public Transform counterAnchor;    // 櫃台位置錨點
    public Transform exerciseAnchor;   // 運動場位置錨點

    [Header("移動設定")]
    [Tooltip("秒數：教練平滑移動及旋轉到目標所需時間")]
    public float moveDuration = 0.5f;

    private void Awake()
    {
        // 一開始就立刻瞬移到櫃台（包含旋轉）
        TeleportToCounter();
    }

    /// <summary> 立即瞬移教練到櫃台（位置 + 旋轉） </summary>
    public void TeleportToCounter()
    {
        StopAllCoroutines();
        coachTransform.position = counterAnchor.position;
        coachTransform.rotation = counterAnchor.rotation;
    }

    /// <summary> 立即瞬移教練到運動場（位置 + 旋轉） </summary>
    public void TeleportToExercise()
    {
        StopAllCoroutines();
        coachTransform.position = exerciseAnchor.position;
        coachTransform.rotation = exerciseAnchor.rotation;
    }

    /// <summary> 平滑移動及旋轉教練到櫃台 </summary>
    public void MoveToCounter()
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(counterAnchor.position, counterAnchor.rotation));
    }

    /// <summary> 平滑移動及旋轉教練到運動場 </summary>
    public void MoveToExercise()
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(exerciseAnchor.position, exerciseAnchor.rotation));
    }

    /// <summary>
    /// 平滑插值 Coroutine：同時對位置與旋轉做 Lerp/Slerp
    /// </summary>
    private IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3   startPos = coachTransform.position;
        Quaternion startRot = coachTransform.rotation;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            coachTransform.position = Vector3.Lerp(startPos, targetPos, t);
            coachTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 確保最後貼齊
        coachTransform.position = targetPos;
        coachTransform.rotation = targetRot;
    }
}
