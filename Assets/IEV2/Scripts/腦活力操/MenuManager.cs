using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button singleButton;
    public Button packageButton;
    public Button backButton;

    public GameObject singlePanel;   // 單項運動自選頁面 Panel
    public GameObject packagePanel;  // 套餐組合運動頁面 Panel

    public UIPanelManager uiPanelManager; 

    private void Start()
    {
        singleButton.onClick.AddListener(OnSingleSelected);
        packageButton.onClick.AddListener(OnPackageSelected);
        backButton.onClick.AddListener(OnBackToMainMenu); // 修改：呼叫返回主選單的方法
    }

    private void OnSingleSelected()
    {
        uiPanelManager.ShowPanel(singlePanel);
    }

    private void OnPackageSelected()
    {
        uiPanelManager.ShowPanel(packagePanel);
    }

    // 【核心修改】這個按鈕現在是返回到「主選單頁面」
    private void OnBackToMainMenu()
    {
        // 呼叫 ReturnToMainMenu() 來清空堆疊並顯示主選單
        uiPanelManager.ReturnToMainMenu();
    }
}