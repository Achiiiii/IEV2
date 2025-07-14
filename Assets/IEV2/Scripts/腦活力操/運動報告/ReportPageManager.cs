using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 雖然此腳本沒直接用，但為了完整性保留

public class ReportPageManager : MonoBehaviour
{
    [Header("按鈕")]
    public Button replayButton;
    public Button backButton;

    [Header("UI 顯示")]
    public Transform actionsContainer;
    public GameObject actionItemPrefab;

    [Header("核心管理器")]
    public UIPanelManager uiPanelManager;
    public ExerciseExecutionManager execManager;
    public CameraSwitcher cameraSwitcher; // 【新增】對攝影機切換器的引用

    [Header("面板切換")]
    public GameObject exerciseExecutionPanel;

    // 再播所需參數
    private List<string> lastActions;
    private bool         lastUseAI;
    private float        lastSpeed;

    private void Awake()
    {
        replayButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        replayButton.onClick.AddListener(OnReplay);
        backButton.onClick.AddListener(OnBack);
    }

    /// <summary>
    /// 由 ExerciseExecutionManager 呼叫，儲存本次參數
    /// </summary>
    public void SetupReport(List<string> actions, bool useAI, float speed)
    {
        lastActions = new List<string>(actions);
        lastUseAI   = useAI;
        lastSpeed   = speed;
        
        // (可選) 產生本次運動的項目列表
        if (actionsContainer != null && actionItemPrefab != null)
        {
            foreach (Transform child in actionsContainer) Destroy(child.gameObject);
            if (actions != null)
            {
                foreach (var actionName in actions)
                {
                    GameObject item = Instantiate(actionItemPrefab, actionsContainer);
                    var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (nameText != null) nameText.text = actionName;
                }
            }
        }
    }

    /// <summary>
    /// 處理「再玩一次」的邏輯
    /// </summary>
    private void OnReplay()
    {
        if (execManager == null)
        {
            Debug.LogError("[ReportPageManager] execManager 尚未設定!");
            return;
        }

        if (lastActions == null || lastActions.Count == 0)
        {
            Debug.LogError("[ReportPageManager] 找不到上次的運動紀錄，無法重玩!");
            OnBack();
            return;
        }

        if (lastActions.Count == 1)
        {
            execManager.StartSingleExercise(lastActions[0], lastUseAI, lastSpeed);
        }
        else
        {
            execManager.StartComboExercise(lastActions, lastUseAI, lastSpeed);
        }
    }

    /// <summary>
    /// 處理返回按鈕的邏輯
    /// </summary>
    private void OnBack()
    {
        // 【修改】在返回主頁前，先將攝影機和教練切回櫃台狀態
        cameraSwitcher?.SwitchToCam1();

        // 【修改】呼叫新的方法，明確地回到「主選單」
        uiPanelManager.ReturnToMainMenu();
    }
}