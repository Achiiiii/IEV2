using System.Collections;
using UnityEngine;

public class CalibrationManager : MonoBehaviour
{
    [Header("Redpill 偵測")]
    public VideoPoseTest poseTest;
    public InputManager inputManager;

    [Header("UI 面板切換")]
    public UIPanelManager uiPanelManager;
    public GameObject mainMenuPanel;

    [Header("校準成功提示")]
    public GameObject successImage;
    [Tooltip("顯示「校準成功」畫面的持續時間（秒）")]
    public float showSuccessDuration = 1.0f;

    [Header("Camera 切換")]
    public CameraSwitcher cameraSwitcher;
    bool calibration_mode= true;

    private void OnEnable()
    {
        // 確保每次進入校準頁面時，都是櫃台視角
        cameraSwitcher.SwitchToCam1();
        inputManager.rawImage.enabled = true;
        poseTest.detectAPose = true;
        poseTest.APoseDetected += HandleAPoseDetected;
    }

    private void OnDisable()
    {
        poseTest.APoseDetected -= HandleAPoseDetected;
    }

    private void Update()
    {
        if (poseTest.detected &&calibration_mode)
        {
            calibration_mode = false;
            HandleAPoseDetected();
        }
        // 方便在 Editor 中測試的後門
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("space");
            HandleAPoseDetected();
            
        }
    }

    public void HandleAPoseDetected()
    {
        StartCoroutine(ShowSuccessAndProceed());
    }


    private IEnumerator ShowSuccessAndProceed()
    {
        if (successImage) successImage.SetActive(true);
        inputManager.rawImage.enabled = false;
        yield return new WaitForSeconds(showSuccessDuration);
        if (successImage) successImage.SetActive(false);

        // 【核心修改】
        // 校準成功後，明確地切換回 Cam1 / 櫃台場景，然後才顯示主頁
        cameraSwitcher.SwitchToCam1();
        uiPanelManager.ShowPanel(mainMenuPanel);
        
        Debug.Log("[CalibrationManager] 校準完成，已切換到主頁面");
    }
}