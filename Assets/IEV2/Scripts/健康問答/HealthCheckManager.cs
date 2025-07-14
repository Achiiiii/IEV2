using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HealthCheckManager : MonoBehaviour
{
    [Header("問答資料")]
    private string[] questions;

    [Header("健康問答 UI 元件")]
    public TextMeshProUGUI questionText;
    public Button yesButton;
    public Button noButton;
    public Button backButton;

    [Header("不予進行結果 Panel")]
    public GameObject notEligiblePanel;
    public Button notEligibleReturnButton;

    // 【核心修改】將原本的項目陣列，改為兩組獨立的勾勾圖片陣列
    [Header("結果頁勾選圖示")]
    [Tooltip("請將7個「是」的勾勾圖片依序拖入")]
    public Image[] yesCheckImages;
    [Tooltip("請將7個「否」的勾勾圖片依序拖入")]
    public Image[] noCheckImages;

    [Header("後續流程 Panel")]
    public GameObject menuPanel;
    public GameObject healthCheckPanel;
    public GameObject mainMenuPanel;

    [Header("整體 UI Panel 管理")]
    public UIPanelManager uiPanelManager;

    // 內部狀態
    private int currentQuestionIndex = 0;
    private bool[] answers;

    private void Awake()
    {
        questions = new string[]
        {
            "問答1: 醫生是否曾經告訴您有心臟病或高血壓 ?",
            "問答2: 您在休息時、日常活動中或進行運動時是否感到胸痛?",
            "問答3: 您是否因頭暈而失去平衡或在過去12個月內失去意識?",
            "問答4: 您是否曾經被診斷患有其他慢性疾病，心臟病或高血壓除外?",
            "問答5: 您目前是否正在服用處方藥治療慢性疾病?",
            "問答6: 您目前是否有骨骼、關節或軟組織問題?",
            "問答7: 醫生是否曾經告訴您，您應該僅在醫療監督下進行運動?"
        };
        answers = new bool[questions.Length];
    }

    private void OnEnable()
    {
        ResetHealthCheck();
    }

    private void ResetHealthCheck()
    {
        currentQuestionIndex = 0;
        for (int i = 0; i < answers.Length; i++)
            answers[i] = false;

        if (notEligiblePanel != null)
            notEligiblePanel.SetActive(false);
        if (healthCheckPanel != null)
            healthCheckPanel.SetActive(true);

        UpdateQuestion();
    }

    private void Start()
    {
        yesButton.onClick.AddListener(() => OnAnswer(true));
        noButton.onClick.AddListener(() => OnAnswer(false));
        backButton.onClick.AddListener(GoBack);
        notEligibleReturnButton.onClick.AddListener(OnNotEligibleReturn);
    }

    private void UpdateQuestion()
    {
        if (questionText != null && currentQuestionIndex < questions.Length)
            questionText.text = questions[currentQuestionIndex];
    }

    private void OnAnswer(bool answer)
    {
        if(currentQuestionIndex < answers.Length)
        {
            answers[currentQuestionIndex] = answer;
        }

        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Length)
        {
            bool anyYes = false;
            foreach (bool ans in answers)
            {
                if (ans)
                {
                    anyYes = true;
                    break;
                }
            }

            if (anyYes)
            {
                ShowNotEligible();
            }
            else
            {
                uiPanelManager.ShowPanel(menuPanel);
            }
        }
        else
        {
            UpdateQuestion();
        }
    }

    /// <summary>
    /// 【核心修改】直接控制兩組 Image 陣列的顯示/隱藏
    /// </summary>
    private void ShowNotEligible()
    {
        if (yesCheckImages != null && noCheckImages != null && 
            yesCheckImages.Length == answers.Length && noCheckImages.Length == answers.Length)
        {
            for (int i = 0; i < answers.Length; i++)
            {
                // 確保對應的 UI 項目存在
                if(yesCheckImages[i] != null)
                    yesCheckImages[i].gameObject.SetActive(answers[i]); // 如果答案是「是」(true)，就顯示「是」的勾勾

                if(noCheckImages[i] != null)
                    noCheckImages[i].gameObject.SetActive(!answers[i]); // 如果答案是「否」(!true)，就顯示「否」的勾勾
            }
        }
        else
        {
            Debug.LogError("結果頁的勾選圖示陣列 (Yes/No Check Images) 未設定或數量不正確 (應為7)！");
        }

        if (healthCheckPanel != null)
            healthCheckPanel.SetActive(false);
        if (notEligiblePanel != null)
            notEligiblePanel.SetActive(true);
    }

    private void OnNotEligibleReturn()
    {
        ResetHealthCheck();
        uiPanelManager.ReturnToMainMenu();
    }

    private void GoBack()
    {
        if (currentQuestionIndex > 0)
        {
            currentQuestionIndex--;
            UpdateQuestion();
        }
        else
        {
            uiPanelManager.ShowPanel(mainMenuPanel);
        }
    }
}