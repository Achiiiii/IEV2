using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.Collections.Generic;

public class ExerciseCarousel : MonoBehaviour
{
    // 定義委派，用於傳遞選擇的動作資訊
    public delegate void OnExerciseSelectedHandler(string exerciseName, VideoClip clip);
    // 宣告公開事件，讓 TeachingModeManager 可以訂閱
    public event OnExerciseSelectedHandler OnExerciseSelected;

    [Header("動作清單 & 影片 (16 項)")]
    public string[] exerciseNames = new string[16]
    {
        "手掌拍肩", "手掌拍肩點腳", "交互拍肩", "交互拍肩點腳",
        "拍手拍膝點腳", "輪流拍膝點腳", "手肘點膝蓋", "指圈互扣",
        "雙指橫向長方形", "雙指直向長方形", "雙指長方形交互",
        "踏步", "踏步畫圈", "手點後腳跟", "臂軀幹後轉", "手點膝"
    };
    public VideoClip[] exerciseClips = new VideoClip[16];

    [Header("輪播 UI")]
    public Button[] slots = new Button[5];
    public Button upArrow;
    public Button downArrow;
    public Sprite normalBackground;
    public Sprite selectedBackground;
    
    [Header("核心管理器與面板")]
    public UIPanelManager uiPanelManager;
    public GameObject teachingModePanel; // 指派 TeachingModeManager 所在的 Panel
    public Button startButton;

    private int currentIndex = 0;

    void Start()
    {
        upArrow.onClick.AddListener(() => { MoveUp(); UpdateCarouselState(); });
        downArrow.onClick.AddListener(() => { MoveDown(); UpdateCarouselState(); });
        startButton.onClick.AddListener(OnConfirmSelection);

        for (int i = 0; i < slots.Length; i++)
        {
            int slot = i;
            slots[i].onClick.AddListener(() =>
            {
                currentIndex = (currentIndex + (slot - 2) + exerciseNames.Length) % exerciseNames.Length;
                UpdateCarouselState();
            });
        }
        UpdateCarouselState(); // 首次初始化
    }

    private void MoveUp() => currentIndex = (currentIndex - 1 + exerciseNames.Length) % exerciseNames.Length;
    private void MoveDown() => currentIndex = (currentIndex + 1) % exerciseNames.Length;

    private void UpdateCarouselState()
    {
        UpdateDisplay();
        
        // 當選擇變更時，觸發 OnExerciseSelected 事件，把新選中的名稱和影片片段傳出去
        if (OnExerciseSelected != null)
        {
             OnExerciseSelected(exerciseNames[currentIndex], exerciseClips[currentIndex]);
        }
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int idx = (currentIndex + (i - 2) + exerciseNames.Length) % exerciseNames.Length;
            slots[i].GetComponentInChildren<TextMeshProUGUI>().text = exerciseNames[idx];
            bool isCenter = (i == 2);
            slots[i].GetComponent<Image>().sprite = isCenter ? selectedBackground : normalBackground;
            slots[i].transform.localScale = isCenter ? Vector3.one * 1.2f : Vector3.one;
        }
    }

    private void OnConfirmSelection()
    {
        // 按下「確認選擇」按鈕，只負責切換到下一個設定頁面
        uiPanelManager.ShowPanel(teachingModePanel);
    }
}