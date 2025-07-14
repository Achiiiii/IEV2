using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 使用「單例模式 (Singleton)」來確保 AudioManager 在整個遊戲中只有一個實例
    public static AudioManager instance;

    [Header("音源 (AudioSource)")]
    private AudioSource bgmSource; // 用於播放背景音樂
    private AudioSource sfxSource; // 用於播放音效

    [Header("音效片段 (AudioClip)")]
    [Tooltip("背景音樂，會循環播放")]
    public AudioClip backgroundMusic;
    [Tooltip("按鈕點擊音效")]
    public AudioClip clickSound;

    private void Awake()
    {
        // --- 單例模式的實現 ---
        // 如果場景中還沒有 AudioManager 的實例
        if (instance == null)
        {
            // 將自己設為實例
            instance = this;
            // 讓這個物件在切換場景時不會被銷毀
            DontDestroyOnLoad(gameObject);
        }
        // 如果場景中已經存在一個 AudioManager，就銷毀自己，避免重複
        else if (instance != this)
        {
            Destroy(gameObject);
            return; // 直接返回，不執行後續的程式碼
        }
        // --- 單例模式結束 ---

        // 用程式碼動態新增 AudioSource 元件，避免需要手動設定
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // 設定背景音樂的 AudioSource
        bgmSource.loop = true; // 設定為循環播放
        bgmSource.playOnAwake = false; // 不要一喚醒就播放
        bgmSource.volume = 0.5f; // 設定一個預設音量
    }

    private void Start()
    {
        // 遊戲一開始就播放背景音樂
        PlayBackgroundMusic();
    }

    /// <summary>
    /// 播放背景音樂
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 播放按鈕點擊音效 (提供給其他腳本呼叫)
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            // PlayOneShot 適合播放短促、可重疊的音效，例如點擊
            sfxSource.PlayOneShot(clickSound);
        }
    }
}