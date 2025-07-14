using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 定義一個套餐的資料結構，方便在 Inspector 中設定
[Serializable]
public class ComboData
{
    public string name;            // 套餐名稱，例如 "專注力訓練"
    public List<string> actions;   // 這個套餐包含的動作名稱清單
}

public class ComboMenuManager : MonoBehaviour
{
    [Header("套餐資料設定")]
    [Tooltip("請在這裡設定六個套餐的名稱與包含的動作")]
    public List<ComboData> combos = new List<ComboData>();

    [Header("UI 元件")]
    public Button[] comboButtons;               // 請在 Inspector 中拖入六顆套餐按鈕
    public UIPanelManager uiPanelManager;       // 面板切換器
    public GameObject detailPanel;              // 要切換過去的「套餐詳細頁」Panel
    public ComboDetailPageManager detailManager;// 「套餐詳細頁」的管理器腳本

    [Header("導航")]
    public Button backToMenuButton;             // 返回主選單的按鈕
    public GameObject mainMenuPanel;            // 主選單 Panel

    void Start()
    {
        if (comboButtons.Length != combos.Count)
            Debug.LogWarning("警告：UI上的按鈕數量與設定的套餐數量不一致！");

        // 遍歷所有按鈕，為它們設定文字與點擊事件
        for (int i = 0; i < comboButtons.Length; i++)
        {
            if (i < combos.Count)
            {
                int index = i; // 閉包(closure)寫法，確保事件能獲取正確的索引
                
                // 設定按鈕上顯示的文字
                var label = comboButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = combos[i].name;
                }

                // 設定點擊事件
                comboButtons[i].onClick.RemoveAllListeners();
                comboButtons[i].onClick.AddListener(() => OnComboSelected(index));
            }
        }

        // 設定返回按鈕
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(() => uiPanelManager.ShowPanel(mainMenuPanel));
        }
    }

    // 當使用者點擊了某個套餐按鈕時
    private void OnComboSelected(int index)
    {
        if (index < 0 || index >= combos.Count)
        {
            Debug.LogError("選擇了無效的套餐索引: " + index);
            return;
        }

        if (detailManager == null || uiPanelManager == null)
        {
            Debug.LogError("[ComboMenuManager] ComboDetailPageManager 或 UIPanelManager 尚未設定!");
            return;
        }

        // 【修改】呼叫 Setup 方法時，現在只傳遞動作列表
        detailManager.Setup(combos[index].actions);
        
        // 切換到「詳細頁」
        uiPanelManager.ShowPanel(detailPanel);
    }
}