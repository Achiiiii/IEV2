using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class ButtonHoverSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("原本與滑入時要替換的圖示")]
    public Sprite normalSprite;    // 預設藍色
    public Sprite hoverSprite;     // 滑鼠滑入時的橘色

    [Header("箭頭指示器 (可選)")]
    public GameObject arrowIndicator; // 滑入時要顯示的箭頭物件

    // 內部參考
    private Image targetImage;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        // 初始化為 normal
        if (normalSprite != null)
            targetImage.sprite = normalSprite;
        if (arrowIndicator != null)
            arrowIndicator.SetActive(false);
    }

    // 滑鼠滑入
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSprite != null)
            targetImage.sprite = hoverSprite;
        if (arrowIndicator != null)
            arrowIndicator.SetActive(true);
    }

    // 滑鼠滑出
    public void OnPointerExit(PointerEventData eventData)
    {
        if (normalSprite != null)
            targetImage.sprite = normalSprite;
        if (arrowIndicator != null)
            arrowIndicator.SetActive(false);
    }
}
