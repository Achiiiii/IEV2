using UnityEngine;
using UnityEngine.UI;

public class StartPageManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject calibrationPanel;
    
    [Header("System References")]
    [SerializeField] private UIPanelManager uiPanelManager;
    
    private void Start()
    {
        InitializeStartPage();
    }
    
    /// <summary>
    /// 初始化起始頁面
    /// </summary>
    private void InitializeStartPage()
    {
        // 設定按鈕點擊事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClick);
        }
        else
        {
            Debug.LogError("StartPageManager: Start Button is not assigned!");
        }
        
        Debug.Log("StartPageManager: 起始頁面初始化完成");
    }
    
    /// <summary>
    /// 處理開始按鈕點擊事件
    /// </summary>
    private void OnStartButtonClick()
    {
        Debug.Log("StartPageManager: Start 按鈕被點擊，切換到定位校準頁面");
        
        // 檢查必要的組件
        if (uiPanelManager == null)
        {
            Debug.LogError("StartPageManager: UIPanelManager is not assigned!");
            return;
        }
        
        if (calibrationPanel == null)
        {
            Debug.LogError("StartPageManager: CalibrationPanel is not assigned!");
            return;
        }
        
        // 切換到定位校準頁面
        uiPanelManager.ShowPanel(calibrationPanel);
    }
    
    private void OnDestroy()
    {
        // 清理按鈕事件
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
        }
    }
}