using UnityEngine;
using UnityEngine.UI;

public class SystemSettingsManager : MonoBehaviour
{
    public static readonly string SpeedSettingKey = "ExerciseSpeed";

    [Header("導航設定")]
    public Button backButton;
    public UIPanelManager uiPanelManager;

    [Header("運動速度選項")]
    public Button speedNormalButton;
    public Button speedSlowButton;
    public Image speedNormalIndicator;
    public Image speedSlowIndicator;
    
    [Header("角色更換選項")]
    public Button charMaleButton;
    public Button charFemaleButton;
    public Image charMaleIndicator;
    public Image charFemaleIndicator;
    
    [Header("角色控制器物件")]
    [Tooltip("請將包含男性角色與Redpill的父物件拖曳到此處")]
    public GameObject maleCharacterController;
    [Tooltip("請將包含女性角色與Redpill的父物件拖曳到此處")]
    public GameObject femaleCharacterController;

    [Header("錯誤提醒選項")]
    public Button reminderOnButton;
    public Button reminderOffButton;
    public Image reminderOnIndicator;
    public Image reminderOffIndicator;

    // --- 內部用來記錄當前設定的變數 ---
    // 【修正】將遺漏的變數宣告加回
    private float currentSpeed;
    private bool isMaleCharacter;
    private bool isReminderOn;


    private void Start()
    {
        backButton.onClick.AddListener(OnBackToMainMenu);
        speedNormalButton.onClick.AddListener(() => SelectSpeed(1.0f));
        speedSlowButton.onClick.AddListener(() => SelectSpeed(0.8f));
        charMaleButton.onClick.AddListener(() => SelectCharacter(true));
        charFemaleButton.onClick.AddListener(() => SelectCharacter(false));
        reminderOnButton.onClick.AddListener(() => SelectReminder(true));
        reminderOffButton.onClick.AddListener(() => SelectReminder(false));

        // 設定頁面載入時的預設選項
        float savedSpeed = PlayerPrefs.GetFloat(SpeedSettingKey, 1.0f);
        SelectSpeed(savedSpeed);
        SelectCharacter(false); // 預設為女角色 (小美)
        SelectReminder(true);
    }

    private void SelectSpeed(float speed)
    {
        currentSpeed = speed; // 現在可以正確賦值
        speedNormalIndicator.gameObject.SetActive(speed == 1.0f);
        speedSlowIndicator.gameObject.SetActive(speed == 0.8f);

        PlayerPrefs.SetFloat(SpeedSettingKey, currentSpeed);
        PlayerPrefs.Save();
        Debug.Log("【系統設定】運動速度已儲存為: " + speed + "x");
    }

    private void SelectCharacter(bool isMale)
    {
        isMaleCharacter = isMale;

        charMaleIndicator.gameObject.SetActive(isMale);
        charFemaleIndicator.gameObject.SetActive(!isMale);

        if (maleCharacterController != null) maleCharacterController.SetActive(isMale);
        if (femaleCharacterController != null) femaleCharacterController.SetActive(!isMale);

        Debug.Log("已切換角色控制器為: " + (isMale ? "男性" : "女性"));
    }

    private void SelectReminder(bool isOn)
    {
        isReminderOn = isOn; // 現在可以正確賦值
        reminderOnIndicator.gameObject.SetActive(isOn);
        reminderOffIndicator.gameObject.SetActive(!isOn);

        Debug.Log("錯誤提醒設定為: " + (isOn ? "ON" : "OFF"));
    }

    private void OnBackToMainMenu()
    {
        uiPanelManager.GoBack();
    }
}