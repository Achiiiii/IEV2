using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboDetailPageManager : MonoBehaviour
{
    [Header("UI 元件")]
    public Transform actionsContainer;
    public GameObject actionItemPrefab;
    
    [Header("設定選項(按鈕)")]
    public Button speed1xButton;
    public Button speed08xButton;
    public Button aiCoachButton;
    public Button videoCoachButton;
    
    [Header("設定選項(指示圖示)")]
    public Image speed1xIndicator;
    public Image speed08xIndicator;
    public Image aiCoachIndicator;
    public Image videoCoachIndicator;
    
    [Header("導航按鈕")]
    public Button startExerciseButton;
    public Button backButton;

    [Header("核心管理器")]
    public UIPanelManager uiPanelManager;
    public ExerciseExecutionManager exerciseExecutionManager;
    
    [Header("面板切換")]
    public GameObject comboMenuPanel;

    // 內部變數
    private List<string> allActionsInCombo;
    private List<Toggle> actionToggles = new List<Toggle>();
    private float selectedSpeed;
    private bool useAI;

    void Start()
    {
        speed1xButton.onClick.AddListener(() => SelectSpeed(1.0f));
        speed08xButton.onClick.AddListener(() => SelectSpeed(0.8f));
        aiCoachButton.onClick.AddListener(() => SelectMode(true));
        videoCoachButton.onClick.AddListener(() => SelectMode(false));
        
        startExerciseButton.onClick.AddListener(OnStartExercise);
        backButton.onClick.AddListener(() => uiPanelManager.ShowPanel(comboMenuPanel));
    }

    /// <summary>
    /// 由 ComboMenuManager 呼叫，用來初始化這個頁面
    /// </summary>
    public void Setup(List<string> actions)
    {
        allActionsInCombo = new List<string>(actions);

        foreach (Transform child in actionsContainer)
        {
            Destroy(child.gameObject);
        }
        actionToggles.Clear();

        foreach (string actionName in allActionsInCombo)
        {
            GameObject item = Instantiate(actionItemPrefab, actionsContainer);
            item.GetComponentInChildren<TextMeshProUGUI>().text = actionName;
            
            Toggle toggle = item.GetComponent<Toggle>();
            if (toggle != null)
            {
                actionToggles.Add(toggle);

                // --- 【核心修改】開始 ---

                // 1. 根據你的 Hierarchy 截圖，找到 Prefab 中的 "Checkmark" 圖示物件
                Transform checkmarkTransform = item.transform.Find("Checkmark");
                if (checkmarkTransform != null)
                {
                    Image checkmarkImage = checkmarkTransform.GetComponent<Image>();
                    
                    // 2. 為 Toggle 的狀態變化事件 (onValueChanged) 加上一個監聽器
                    //    這個監聽器會在每次 Toggle 被點擊、狀態改變時自動執行
                    toggle.onValueChanged.AddListener((isOn) => 
                    {
                        // 參數 isOn 會傳入 Toggle 的最新狀態 (true 或 false)
                        // 我們用這個狀態來直接控制 Checkmark 圖片的啟用/禁用
                        if(checkmarkImage != null)
                        {
                            checkmarkImage.gameObject.SetActive(isOn);
                        }
                    });

                    // 3. 設定初始狀態
                    toggle.isOn = true;
                    // 確保 Checkmark 圖片的初始狀態也是可見的
                    if(checkmarkImage != null)
                    {
                        checkmarkImage.gameObject.SetActive(true);
                    }
                }
                else
                {
                    Debug.LogWarning("在運動選項 Prefab 中找不到名為 'Checkmark' 的子物件，勾選的視覺效果可能無法正常顯示。");
                }
                // --- 【核心修改】結束 ---
            }
        }

        // 初始化時，讀取全域設定的速度
        float defaultSpeed = PlayerPrefs.GetFloat(SystemSettingsManager.SpeedSettingKey, 1.0f);
        SelectSpeed(defaultSpeed); 

        // 初始化預設教學模式
        SelectMode(true);
    }

    /// <summary>
    /// 選擇速度 (單次覆寫，不儲存)
    /// </summary>
    private void SelectSpeed(float speed)
    {
        selectedSpeed = speed;
        speed1xIndicator.enabled = (speed == 1.0f);
        speed08xIndicator.enabled = (speed == 0.8f);
    }

    private void SelectMode(bool isAI)
    {
        useAI = isAI;
        aiCoachIndicator.enabled = isAI;
        videoCoachIndicator.enabled = !isAI;
    }

    private void OnStartExercise()
    {
        List<string> selectedActions = new List<string>();
        for (int i = 0; i < actionToggles.Count; i++)
        {
            if (actionToggles[i].isOn)
            {
                selectedActions.Add(allActionsInCombo[i]);
            }
        }

        if (selectedActions.Count == 0)
        {
            Debug.LogWarning("未選擇任何運動項目！");
            return;
        }

        exerciseExecutionManager.StartComboExercise(selectedActions, useAI, selectedSpeed);
    }
}