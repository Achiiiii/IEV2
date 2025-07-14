using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

// ExerciseMapping class 定義
[Serializable]
public class ExerciseMapping
{
    public string actionName;
    public VideoClip videoClip;
    public string animationStateName;
}

public class ExerciseExecutionManager : MonoBehaviour
{
    [Header("進度列表UI")]
    public Transform stepsContainer;
    public GameObject stepItemPrefab;
    [Tooltip("正在執行中的項目要放大的倍率")]
    public float highlightedStepScale = 1.2f;
    [Tooltip("已完成的項目要縮小的倍率")]
    public float completedStepScale = 0.8f;
    [Tooltip("執行中項目的文字顏色")]
    public Color highlightedStepColor = Color.white;
    [Tooltip("非執行中(已完成/未執行)項目的文字顏色")]
    public Color normalStepColor = Color.gray;

    [Header("提示圖片 (取代文字)")]
    public Image startingHintImage;
    public Image nextHintImage;
    public Image finishedHintImage;

    [Header("倒數圖片 (5,4,3,2,1,GO)")]
    [Tooltip("陣列大小為6，請依序放入 5,4,3,2,1,GO 的圖片")]
    public Image[] countdownImages;

    [Header("教學模式切換 UI")]
    public Button aiToggleButton;
    public Button videoToggleButton;
    public Image aiArrow;
    public Image videoArrow;

    [Header("影片播放設定")]
    public RenderTexture videoRT;
    public RawImage videoRawImage;
    public VideoPlayer videoPlayer;

    [Header("使用者視訊")]
    public RawImage userRawImage;

    [Header("畫面外框")]
    public Image videoFrameImage;
    public Image userFrameImage;
    
    [Header("畫面標題")]
    public GameObject userTitleObject;
    public GameObject coachTitleObject;
    
    [Header("角色名牌")]
    public GameObject userNameplateObject;
    public GameObject coachNameplateObject;

    [Header("遮罩 Mask")]
    public GameObject videoMask;
    public GameObject userMask;

    [Header("3D 模型與動畫")]
    public Transform coachModelRoot;
    public Animator coachAnimator;
    public GameObject maleUserObject;
    public GameObject femaleUserObject;

    [Header("導航與面板")]
    public Button backButton;
    public GameObject reportPanel;
    public UIPanelManager uiPanelManager;

    [Header("報告管理")]
    public ReportPageManager reportPageManager;

    [Header("動作對應表")]
    public List<ExerciseMapping> mappings;

    [Header("倒數秒數")]
    public int initialCountdown = 5;

    [Header("動作間緩衝秒數")]
    public int bufferSeconds = 5;

    [Header("教練位置管理")]
    public CoachPositionManager coachPositionManager;

    [Header("鏡頭切換")]
    public CameraSwitcher cameraSwitcher;

    [Header("暫停功能")]
    public GameObject pausePanel;
    public Button returnToMenuButton;
    public Button resumeButton;

    // --- 常數定義 ---
    private const int COUNTDOWN_IMAGES_COUNT = 6;
    private const float FINISHED_HINT_DISPLAY_TIME = 3f;
    private const float COUNTDOWN_INTERVAL = 1f;
    private const float MIN_PLAY_SPEED = 0.1f;

    // --- 內部狀態變數 ---
    private CanvasGroup canvasGroup;
    private bool isPaused = false;
    private List<string> exerciseQueue = new List<string>();
    private bool useAI;
    private float playSpeed;
    private bool isReadyToStart = false;
    private bool isActionPlaying = false; 
    private Renderer[] coachRenderers;
    private Renderer[] userRenderers;
    private List<GameObject> stepItemObjects = new List<GameObject>();
    private List<TextMeshProUGUI> stepNameTexts = new List<TextMeshProUGUI>();
    private List<Image> stepProgressBars = new List<Image>();
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning("在 ExerciseExecutionManager 上找不到 CanvasGroup，已自動添加。");
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (coachModelRoot != null) 
        {
            coachRenderers = coachModelRoot.GetComponentsInChildren<Renderer>(true);
        }

        if (videoPlayer != null && videoRT != null) 
        { 
            videoPlayer.renderMode = VideoRenderMode.RenderTexture; 
            videoPlayer.targetTexture = videoRT; 
        }
        
        if (videoRawImage != null && videoRT != null) 
        {
            videoRawImage.texture = videoRT;
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
        }
    }
    
    private void OnEnable()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (isReadyToStart)
        {
            isReadyToStart = false; 
            StartCoroutine(FlowRoutine());
        }
    }

    private void Start()
    {
        if (aiToggleButton != null) aiToggleButton.onClick.AddListener(() => SwitchDisplay(true));
        if (videoToggleButton != null) videoToggleButton.onClick.AddListener(() => SwitchDisplay(false));
        if (backButton != null) backButton.onClick.AddListener(TogglePause);
        if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(OnPauseReturnToMenu);
        if (resumeButton != null) resumeButton.onClick.AddListener(TogglePause);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        if (aiToggleButton != null) aiToggleButton.onClick.RemoveAllListeners();
        if (videoToggleButton != null) videoToggleButton.onClick.RemoveAllListeners();
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (returnToMenuButton != null) returnToMenuButton.onClick.RemoveAllListeners();
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
    }

    public void StartSingleExercise(string exerciseName, bool isAIMode, float speed)
    {
        if (string.IsNullOrEmpty(exerciseName))
        {
            Debug.LogError("動作名稱不能為空");
            return;
        }
        
        var exerciseList = new List<string> { exerciseName };
        InitExecution(exerciseList, isAIMode, speed);
    }

    public void StartComboExercise(List<string> exerciseNames, bool isAIMode, float speed)
    {
        InitExecution(exerciseNames, isAIMode, speed);
    }

    private void InitExecution(List<string> selectedActions, bool aiMode, float speed)
    {
        if (selectedActions == null || selectedActions.Count == 0)
        {
            Debug.LogError("動作列表為空，無法開始執行");
            if (uiPanelManager != null) uiPanelManager.ReturnToMainMenu();
            return;
        }
        
        if (speed < MIN_PLAY_SPEED)
        {
            Debug.LogWarning($"播放速度 {speed} 太小，已設為最小值 {MIN_PLAY_SPEED}");
            speed = MIN_PLAY_SPEED;
        }
        
        if (mappings == null || mappings.Count == 0)
        {
            Debug.LogError("動作對應表 (mappings) 為空，無法執行動作");
            if (uiPanelManager != null) uiPanelManager.ReturnToMainMenu();
            return;
        }

        StopAllCoroutines();
        this.exerciseQueue = new List<string>(selectedActions);
        this.useAI = aiMode;
        this.playSpeed = speed;

        isPaused = false;
        isActionPlaying = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        InitializeUIElements();
        SetupUserCharacter();
        SwitchDisplay(this.useAI);
        
        if (cameraSwitcher != null) cameraSwitcher.SwitchToCam2();
        if (coachPositionManager != null) coachPositionManager.MoveToExercise();
        
        if (coachAnimator != null) 
        { 
            coachAnimator.Rebind(); 
            coachAnimator.Update(0); 
        }

        BuildStepList();
        
        var firstMap = mappings.Find(m => m.actionName == this.exerciseQueue[0]);
        if (firstMap == null)
        {
            Debug.LogError($"找不到動作 '{this.exerciseQueue[0]}' 的對應設定");
            if (uiPanelManager != null) uiPanelManager.ReturnToMainMenu();
            return;
        }

        SetupFirstFrame(firstMap);
        
        isReadyToStart = true;
        
        if (uiPanelManager != null) uiPanelManager.ShowPanel(this.gameObject);
    }

    private void InitializeUIElements()
    {
        if (startingHintImage != null) startingHintImage.gameObject.SetActive(false);
        if (nextHintImage != null) nextHintImage.gameObject.SetActive(false);
        if (finishedHintImage != null) finishedHintImage.gameObject.SetActive(false);
        
        if (countdownImages != null)
        {
            foreach(var img in countdownImages)
            {
                if (img != null)
                {
                    img.gameObject.SetActive(false);
                    img.raycastTarget = false;
                }
            }
        }
        
        if (startingHintImage != null) startingHintImage.raycastTarget = false;
        if (nextHintImage != null) nextHintImage.raycastTarget = false;
        if (finishedHintImage != null) finishedHintImage.raycastTarget = false;
    }

    private void SetupUserCharacter()
    {
        if (maleUserObject != null && maleUserObject.activeInHierarchy)
        {
            userRenderers = maleUserObject.GetComponentsInChildren<Renderer>(true);
            Debug.Log("偵測到目前使用者為：男角色");
        }
        else if (femaleUserObject != null && femaleUserObject.activeInHierarchy)
        {
            userRenderers = femaleUserObject.GetComponentsInChildren<Renderer>(true);
            Debug.Log("偵測到目前使用者為：女角色");
        }
        else
        {
            userRenderers = null;
            Debug.LogWarning("找不到任何啟用的使用者角色物件！");
        }
    }

    private IEnumerator FlowRoutine()
    {
        isActionPlaying = false; 

        UpdateStepUI(0);
        
        var firstMapOnStart = mappings.Find(m => m.actionName == this.exerciseQueue[0]);
        if (firstMapOnStart != null)
        {
            SetupFirstFrame(firstMapOnStart);
        }

        if (startingHintImage != null) startingHintImage.gameObject.SetActive(true);
        yield return StartCoroutine(CountdownRoutine());
        if (startingHintImage != null) startingHintImage.gameObject.SetActive(false);

        for (int idx = 0; idx < exerciseQueue.Count; idx++)
        {
            UpdateStepUI(idx);
            var map = mappings.Find(m => m.actionName == exerciseQueue[idx]);

            if (map == null) 
            {
                Debug.LogError($"錯誤：在 Mappings 列表中找不到名為 '{exerciseQueue[idx]}' 的動作，將跳過此動作。");
                continue;
            }

            float duration = GetDuration(map);
            if (duration > 0 && idx < stepProgressBars.Count && stepProgressBars[idx] != null)
            {
                StartCoroutine(FillProgressBar(stepProgressBars[idx], duration));
            }
            
            isActionPlaying = true;
            
            if (coachAnimator != null && !string.IsNullOrEmpty(map.animationStateName))
            {
                coachAnimator.Play(map.animationStateName, 0, 0f);
                coachAnimator.speed = playSpeed;
            }
            
            if (videoPlayer != null && map.videoClip != null)
            {
                videoPlayer.clip = map.videoClip;
                videoPlayer.time = 0;
                videoPlayer.playbackSpeed = playSpeed;
                videoPlayer.Play();
            }
            
            if (useAI)
            {
                yield return StartCoroutine(WaitAnimationFinish(map.animationStateName));
            }
            else
            {
                yield return StartCoroutine(WaitVideoFinish());
            }
            
            isActionPlaying = false;
            
            if (videoPlayer != null) videoPlayer.Stop();
            if (coachAnimator != null) coachAnimator.speed = 0f;

            if (idx < exerciseQueue.Count - 1)
            {
                var nextMap = mappings.Find(m => m.actionName == exerciseQueue[idx + 1]);
                if (nextMap != null) SetupFirstFrame(nextMap);
                
                UpdateStepUI(idx + 1);

                if (nextHintImage != null) nextHintImage.gameObject.SetActive(true);
                yield return StartCoroutine(CountdownRoutine());
                if (nextHintImage != null) nextHintImage.gameObject.SetActive(false);
            }
        }
        
        if (finishedHintImage != null) finishedHintImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(FINISHED_HINT_DISPLAY_TIME);
        if (finishedHintImage != null) finishedHintImage.gameObject.SetActive(false);

        if (reportPageManager != null) 
        {
            reportPageManager.SetupReport(exerciseQueue, useAI, playSpeed);
        }
        
        if (uiPanelManager != null && reportPanel != null)
        {
            uiPanelManager.ShowPanel(reportPanel);
        }
        
        if (coachAnimator != null) 
        { 
            coachAnimator.Rebind(); 
            coachAnimator.Play("Idle", 0, 0f); 
            coachAnimator.speed = 1f; 
        }
    }

    private IEnumerator CountdownRoutine()
    {
        if (countdownImages != null && countdownImages.Length >= COUNTDOWN_IMAGES_COUNT)
        {
            for (int i = 0; i < 5; i++)
            {
                if (countdownImages[i] != null) 
                { 
                    countdownImages[i].gameObject.SetActive(true); 
                }
                yield return new WaitForSeconds(COUNTDOWN_INTERVAL);
                if (countdownImages[i] != null) 
                { 
                    countdownImages[i].gameObject.SetActive(false); 
                }
            }
            
            if (countdownImages[5] != null) 
            { 
                countdownImages[5].gameObject.SetActive(true); 
            }
            yield return new WaitForSeconds(COUNTDOWN_INTERVAL);
            if (countdownImages[5] != null) 
            { 
                countdownImages[5].gameObject.SetActive(false); 
            }
        }
        else
        {
            Debug.LogError($"倒數圖片陣列 (Countdown Images) 未設定或長度不足{COUNTDOWN_IMAGES_COUNT}！");
            yield return new WaitForSeconds(initialCountdown);
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
        
        Time.timeScale = isPaused ? 0f : 1f;

        if (isActionPlaying)
        {
            if (isPaused)
            {
                if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Pause();
            }
            else
            {
                if (videoPlayer != null && videoPlayer.isPaused) videoPlayer.Play();
            }
        }
    }

    private void OnPauseReturnToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        StopAllCoroutines();
        
        if (videoPlayer != null) videoPlayer.Stop();
        
        if (coachAnimator != null)
        {
            coachAnimator.Rebind();
            coachAnimator.Play("Idle", 0, 0f);
            coachAnimator.speed = 1f;
        }

        if (uiPanelManager != null) uiPanelManager.ReturnToMainMenu();
        if (cameraSwitcher != null) cameraSwitcher.SwitchToCam1();
    }
    
    private void BuildStepList()
    {
        if (stepsContainer == null) return;
        
        stepItemObjects.Clear();
        stepNameTexts.Clear();
        stepProgressBars.Clear();
        foreach (Transform c in stepsContainer) { if (c != null) Destroy(c.gameObject); }
        
        for (int i = 0; i < exerciseQueue.Count; i++)
        {
            if (stepItemPrefab == null) return;
            var go = Instantiate(stepItemPrefab, stepsContainer);
            stepItemObjects.Add(go);

            var tmps = go.GetComponentsInChildren<TextMeshProUGUI>();
            
            foreach (var t in tmps)
            {
                if (t.gameObject.name.Contains("Number")) 
                { 
                    t.text = (i + 1).ToString("00"); 
                }
                else if (t.gameObject.name.Contains("Name"))
                {
                    t.text = exerciseQueue[i];
                    stepNameTexts.Add(t);
                }
            }
            
            var barTrans = go.transform.Find("ProgressBar");
            if (barTrans != null)
            {
                Image bar = barTrans.GetComponent<Image>();
                if (bar != null)
                {
                    bar.type = Image.Type.Filled;
                    bar.fillMethod = Image.FillMethod.Horizontal;
                    bar.fillAmount = 0f;
                    stepProgressBars.Add(bar);
                }
                else { stepProgressBars.Add(null); }
            }
            else { stepProgressBars.Add(null); }
        }
    }

    private IEnumerator FillProgressBar(Image bar, float duration)
    {
        if (bar == null || duration <= 0f) yield break;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!isPaused)
            {
                elapsed += Time.deltaTime;
                bar.fillAmount = Mathf.Clamp01(elapsed / duration);
            }
            yield return null;
        }
        bar.fillAmount = 1f;
    }

    private float GetDuration(ExerciseMapping map)
    {
        if (map == null) return 0f;
        
        float currentPlaySpeed = Mathf.Max(playSpeed, MIN_PLAY_SPEED);
        
        if (useAI)
        {
            return GetAnimationClipLength(map.animationStateName) / currentPlaySpeed;
        }
        else
        {
            return map.videoClip != null ? (float)(map.videoClip.length / currentPlaySpeed) : 0f;
        }
    }

    private float GetAnimationClipLength(string stateName)
    {
        if (string.IsNullOrEmpty(stateName) || coachAnimator == null) return 0f;
        
        if (coachAnimator.runtimeAnimatorController == null) return 0f;
        
        foreach (var clip in coachAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == stateName) 
            {
                return clip.length;
            }
        }
        
        Debug.LogWarning($"在 Animator Controller 中找不到名為 '{stateName}' 的 Animation Clip。");
        return 0f;
    }

    /// <summary>
    /// 【核心修改】更新進度列表所有項目的UI狀態 (顏色與縮放)
    /// </summary>
    private void UpdateStepUI(int highlightIndex)
    {
        if (stepItemObjects == null) return;

        for (int i = 0; i < stepItemObjects.Count; i++)
        {
            if (stepItemObjects[i] == null) continue;

            Color targetColor;
            float targetScale;

            if (i < highlightIndex)
            {
                // 已完成的項目
                targetColor = normalStepColor;
                targetScale = completedStepScale;
            }
            else if (i == highlightIndex)
            {
                // 正在執行的項目
                targetColor = highlightedStepColor;
                targetScale = highlightedStepScale;
            }
            else
            {
                // 未執行的項目
                targetColor = normalStepColor;
                targetScale = 1.0f;
            }

            // 應用顏色
            if (i < stepNameTexts.Count && stepNameTexts[i] != null)
            {
                stepNameTexts[i].color = targetColor;
            }

            // 應用縮放
            stepItemObjects[i].transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }
    }

    private void SetupFirstFrame(ExerciseMapping map)
    {
        if (map == null) return;
        
        if (map.videoClip != null && videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = map.videoClip;
            videoPlayer.time = 0;
            videoPlayer.Prepare();
            videoPlayer.Pause();
        }
        
        if (!string.IsNullOrEmpty(map.animationStateName) && coachAnimator != null)
        {
            coachAnimator.speed = 1f;
            coachAnimator.Play(map.animationStateName, 0, 0f);
            coachAnimator.Update(0);
            coachAnimator.speed = 0f;
        }
    }

    private IEnumerator WaitVideoFinish()
    {
        if (videoPlayer == null || videoPlayer.clip == null) yield break;
        
        float duration = (float)videoPlayer.clip.length / Mathf.Max(playSpeed, MIN_PLAY_SPEED);
        float elapsed = 0f;
        
        while(elapsed < duration)
        {
            if (!isPaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
    }

    private IEnumerator WaitAnimationFinish(string stateName)
    {
        if (string.IsNullOrEmpty(stateName) || coachAnimator == null) yield break;
        
        float duration = GetAnimationClipLength(stateName) / Mathf.Max(playSpeed, MIN_PLAY_SPEED);
        float elapsed = 0f;
        
        while(elapsed < duration)
        {
            if (!isPaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
    }

    private void SwitchDisplay(bool isAIMode)
    {
        this.useAI = isAIMode;
        
        if(aiArrow != null) aiArrow.gameObject.SetActive(isAIMode);
        if(videoArrow != null) videoArrow.gameObject.SetActive(!isAIMode);
        
        if (coachRenderers != null) { foreach (var r in coachRenderers) { if (r != null) r.enabled = isAIMode; } }
        if (userRenderers != null) { foreach (var r in userRenderers) { if (r != null) r.enabled = isAIMode; } }
        
        bool showVideo = !isAIMode;
        if (videoRawImage != null) videoRawImage.enabled = showVideo;
        if (userRawImage != null) userRawImage.enabled = showVideo;
        if (videoFrameImage != null) videoFrameImage.enabled = showVideo;
        if (userFrameImage != null) userFrameImage.enabled = showVideo;
        if (videoMask != null) videoMask.SetActive(showVideo);
        if (userMask != null) userMask.SetActive(showVideo);
        
        if (userTitleObject != null) userTitleObject.SetActive(showVideo);
        if (coachTitleObject != null) coachTitleObject.SetActive(showVideo);
        
        if (userNameplateObject != null) userNameplateObject.SetActive(isAIMode);
        if (coachNameplateObject != null) coachNameplateObject.SetActive(isAIMode);
    }
}