using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TeachingModeManager : MonoBehaviour
{
    [Header("核心管理器")]
    public UIPanelManager uiPanelManager;
    public ExerciseExecutionManager exerciseExecutionManager;
    public ExerciseCarousel exerciseCarousel;

    [Header("UI 元件")]
    public Button startButton;
    public Button backButton;
    public Button aiCoachButton;
    public Button videoCoachButton;
    public Image aiCheckImage;
    public Image videoCheckImage;

    [Header("影片預覽")]
    public VideoPlayer previewVideoPlayer;
    [Tooltip("預設顯示的第一個動作影片 (例如：手掌拍肩)")]
    public VideoClip defaultPreviewClip; // 【新增】用於設定預設預覽影片

    [Header("面板切換")]
    public GameObject exerciseChoicePanel; 

    // 內部狀態
    private string selectedExercise;
    private bool useAI;
    private float selectedSpeed = 1.0f;

    // 【新增】OnEnable 方法
    private void OnEnable()
    {
        // 當頁面被啟用時，設定並播放預設的預覽影片
        // 我們也預設第一個動作的名稱為 "手掌拍肩"
        if (defaultPreviewClip != null)
        {
            InitializePreview("手掌拍肩", defaultPreviewClip);
        }

        // 同時重置模式為預設的 AI 教練
        SelectMode(true);
    }

    void Start()
    {
        // 訂閱輪播選單的事件，當選擇變更時，自動更新預覽
        if (exerciseCarousel != null)
        {
            exerciseCarousel.OnExerciseSelected += InitializePreview;
        }

        // 為所有按鈕綁定事件
        startButton.onClick.AddListener(OnStartExercise);
        backButton.onClick.AddListener(OnBack);
        aiCoachButton.onClick.AddListener(() => SelectMode(true));
        videoCoachButton.onClick.AddListener(() => SelectMode(false));
    }

    /// <summary>
    /// 由 ExerciseCarousel 的事件觸發，或在 OnEnable 時呼叫，用來更新預覽畫面
    /// </summary>
    public void InitializePreview(string exerciseName, VideoClip clip)
    {
        selectedExercise = exerciseName;
        if (previewVideoPlayer != null)
        {
            previewVideoPlayer.clip = clip;
            previewVideoPlayer.Play();
        }
    }

    // 選擇教學模式
    private void SelectMode(bool isAI)
    {
        useAI = isAI;
        // 更新 UI 上的勾勾顯示
        aiCheckImage.gameObject.SetActive(isAI);
        videoCheckImage.gameObject.SetActive(!isAI);
    }

    // 按下「返回」按鈕
    private void OnBack()
    {
        uiPanelManager.ShowPanel(exerciseChoicePanel);
    }

    // 按下「開始運動」按鈕
    private void OnStartExercise()
    {
        exerciseExecutionManager.StartSingleExercise(selectedExercise, useAI, selectedSpeed);
    }
}