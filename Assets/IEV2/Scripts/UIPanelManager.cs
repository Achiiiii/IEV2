using System.Collections.Generic;
using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    [Header("頁面參考")]
    [Tooltip("應用程式的初始進入點，通常是 Logo 或開始頁面")]
    public GameObject startPanel; 
    [Tooltip("應用程式的主選單頁面 (有單項/套餐選項的頁面)")]
    public GameObject mainMenuPanel; 

    private Stack<GameObject> panelStack = new Stack<GameObject>();

    private void Start()
    {
        // 應用程式啟動時，清空所有記錄並只顯示開始頁面
        while (panelStack.Count > 0)
        {
            panelStack.Pop().SetActive(false);
        }

        if (startPanel != null)
        {
            panelStack.Push(startPanel);
            startPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[UIPanelManager] 開始頁面 (Start Panel) 未設定！");
        }
    }

    /// <summary> 顯示一個新的 Panel，並隱藏目前的頁面 </summary>
    public void ShowPanel(GameObject newPanel)
    {
        if (newPanel == null) 
        {
            Debug.LogError("[UIPanelManager] 嘗試顯示一個空的 Panel！");
            return;
        }

        if (panelStack.Count > 0)
        {
            panelStack.Peek().SetActive(false);
        }
        panelStack.Push(newPanel);
        newPanel.SetActive(true);
    }

    /// <summary> 返回上一頁 </summary>
    public void GoBack()
    {
        if (panelStack.Count <= 1) return; 

        GameObject currentPanel = panelStack.Pop();
        currentPanel.SetActive(false);

        GameObject previousPanel = panelStack.Peek();
        previousPanel.SetActive(true);
    }

    /// <summary> 【新】清空歷史紀錄並回到「開始頁」 </summary>
    public void ReturnToStartPage()
    {
        while (panelStack.Count > 0)
        {
            panelStack.Pop().SetActive(false);
        }
        ShowPanel(startPanel);
    }
    
    /// <summary> 【新】清空歷史紀錄並回到「主選單」 </summary>
    public void ReturnToMainMenu()
    {
        while (panelStack.Count > 0)
        {
            panelStack.Pop().SetActive(false);
        }
        ShowPanel(mainMenuPanel);
    }
}