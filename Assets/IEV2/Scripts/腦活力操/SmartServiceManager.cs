using UnityEngine;
using UnityEngine.UI;

public class SmartServiceManager : MonoBehaviour
{
    [Header("UI 元件")]
    [Tooltip("返回按鈕")]
    public Button backButton;

    [Header("核心管理器")]
    [Tooltip("UI Panel 管理器")]
    public UIPanelManager uiPanelManager;


    void Start()
    {
        // 確保 backButton 已被指派，然後為其綁定點擊事件
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackToMainMenu);
        }
        else
        {
            Debug.LogError("智能客服頁面的返回按鈕 (backButton) 尚未在 Inspector 中設定！");
        }
    }

    /// <summary>
    /// 處理返回主選單的點擊事件
    /// </summary>
    private void OnBackToMainMenu()
    {
        if (uiPanelManager != null)
        {
            // GoBack() 會關閉當前頁面，並顯示頁面堆疊中的上一個頁面（即主選單）
            uiPanelManager.GoBack();
        }
        else
        {
            Debug.LogError("UIPanelManager 尚未在 Inspector 中設定！");
        }
    }
}