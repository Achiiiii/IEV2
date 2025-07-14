using UnityEngine;
using UnityEngine.UI;
using Tobii.Research.Unity;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening; // �K�[�o�Ӥޥ�

public class GazePointVisualizer : MonoBehaviour
{
    [SerializeField] private RectTransform gazeDot;       //  UI 點
    [SerializeField] private RectTransform canvasTransform; // UI canvas（需設為 Screen Space - Overlay）
    [SerializeField]
    [Tooltip("This key will toggle gaze point.")]
    private KeyCode _toggleKey = KeyCode.None;
    private Image _gazeDotImage;

    private int _windowSize = 10;
    private Queue<Vector2> _positionBuffer = new Queue<Vector2>();
    private bool _isDotShow = true;
    private float scaleRatio = 1;

    void Start()
    {
        _gazeDotImage = gazeDot.gameObject.GetComponent<Image>();
    }

    void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleGazeDot(!_isDotShow);
        }
        var gazeData = EyeTracker.Instance.LatestGazeData;

        if (gazeData == null || gazeData.Left == null || gazeData.Right == null)
            return;

        // 檢查左右眼資料是否有效
        if (!gazeData.Left.GazePointValid || !gazeData.Right.GazePointValid)
            return;

        // 計算左右眼的平均 gaze 點（座標在 display area，值域為 0~1）
        Vector2 leftPoint = gazeData.Left.GazePointOnDisplayArea;
        Vector2 rightPoint = gazeData.Right.GazePointOnDisplayArea;
        Vector2 avgGazePoint = (leftPoint + rightPoint) / 2f;

        // 轉換為螢幕座標系（0,0 左下角）
        Vector2 curPos = new Vector2(
            avgGazePoint.x * Screen.width,
            (1f - avgGazePoint.y) * Screen.height); // y 軸要翻轉

        // Debug.Log(screenPos);
        // Debug.Log(gazeData.TimeStamp);

        _positionBuffer.Enqueue(curPos);

        if (_positionBuffer.Count > _windowSize)
        {
            _positionBuffer.Dequeue();
        }

        var smoothedPos = _positionBuffer.Aggregate(Vector2.zero, (sum, next) => sum + next) / _positionBuffer.Count;

        // 將螢幕座標轉換為 Canvas 的 Local Position（anchoredPosition）
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform, smoothedPos, null, out Vector2 canvasPos);

        // 移動 gazeDot UI 點
        gazeDot.anchoredPosition = canvasPos / scaleRatio;
    }

    public void ToggleGazeDot(bool value)
    {
        if (value)
        {
            _gazeDotImage.DOFade(1, 0);
        }
        else
        {
            _gazeDotImage.DOFade(0, 0);
        }
        _isDotShow = value;
    }

    public void SetRatio(float value)
    {
        scaleRatio = value;
    }
}
