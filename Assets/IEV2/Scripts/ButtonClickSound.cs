using UnityEngine;
using UnityEngine.UI;

// 這個腳本需要附加在有 Button 元件的 GameObject 上
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        // 取得自己身上的 Button 元件
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // 當按鈕啟用時，為它的點擊事件加上播放音效的監聽
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    private void OnDisable()
    {
        // 當按鈕被禁用時，移除監聽，是一個好習慣
        if (button != null)
        {
            button.onClick.RemoveListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        // 呼叫 AudioManager 的單例來播放點擊音效
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayClickSound();
        }
    }
}