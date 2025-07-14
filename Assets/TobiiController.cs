using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TobiiController : MonoBehaviour
{
    public RectTransform eyeTrackRect;
    public TMPro.TMP_Text content;
    public TMPro.TMP_Text contentUnder;
    public GameObject blackTestBtn;
    public GameObject colorTestBtn;
    public GameObject startBtn;
    public GameObject blackTest;
    public GameObject colorTest;
    public GameObject backBtn;
    public GameObject[] stars;
    public Sprite starFull;
    public Sprite starEmpty;
    public GazePointVisualizer gazePointVisualizer;

    [SerializeField]
    [Tooltip("This key will show or hide the track box guide.")]
    private KeyCode _toggleKey = KeyCode.None;

    private string mode;
    private float testDuration;
    private float startTime;
    private float endTime;
    private bool isScale = true;

    void Start()
    {
        gazePointVisualizer.SetRatio(eyeTrackRect.localScale.x);
    }

    void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            if (isScale)
            {
                eyeTrackRect.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                eyeTrackRect.localScale = new Vector3(0.42f, 0.42f, 1);
            }
            gazePointVisualizer.SetRatio(eyeTrackRect.localScale.x);
            isScale = !isScale;
        }
    }

    public void BlackTestClick()
    {
        HomePageControl(
            false,
            "請依照數字順序（1 到 25），將視線依序注視螢幕上隨機分布的圓圈。每注視正確的數字，系統會自動連線；錯誤則不顯示任何反應。\n\n。準備好後，請凝視開始5秒。",
            "測驗時間：約 50 秒至 20 分鐘"
        );
        mode = "black";
    }
    public void ColorTestClick()
    {
        HomePageControl(
            false,
            "請依照圓圈內的數字順序，從 1 到 25 進行注視，並交替使用紅色與黃色數字（紅1 > 黃2 > 紅3 > 黃4...）。圓圈隨機分佈於畫面中，注視正確會自動連線，錯誤則無任何反應。\n\n。準備好後，請凝視開始5秒。",
            "測驗時間：約 3 至 8 分鐘。"
        );
        mode = "color";
    }
    public void TestBackBtn()
    {
        HomePageControl(
            true,
            "請問您今天想進行哪一項眼動測試呢？\n（凝視選項5秒）",
            ""
        );
        TestPageControl(false);
    }

    public void StartClick()
    {
        gazePointVisualizer.ToggleGazeDot(false);
        TestPageControl(true);
        startTime = Time.time;
    }

    private void TestPageControl(bool value)
    {
        switch (mode)
        {
            case "black":
                blackTest.SetActive(value);
                break;
            case "color":
                colorTest.SetActive(value);
                break;
            default:
                break;
        }
    }
    private void HomePageControl(bool value, string _content, string _contentUnder)
    {
        blackTestBtn.SetActive(value);
        colorTestBtn.SetActive(value);
        startBtn.SetActive(!value);
        backBtn.SetActive(value);
        content.text = _content;
        contentUnder.text = _contentUnder;
    }
    private void ShowResult(int score)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(true);
            if (i <= score) stars[i].GetComponent<Image>().sprite = starFull;
            else stars[i].GetComponent<Image>().sprite = starEmpty;
        }
        switch (score)
        {
            case 0:
                content.text = "測驗結束！\n您的測驗結果需要再加油，持續練習會讓您越來越進步。";
                break;
            case 1:
                content.text = "測驗結束！\n您已達到不錯的水準！相信您能越來越好！";
                break;
            case 2:
                content.text = "測驗結束！\n您的測驗結果非常優秀！請繼續保持！";
                break;
            default:
                break;
        }
        StartCoroutine(DelayHideResult());
    }

    private IEnumerator DelayHideResult()
    {
        yield return new WaitForSeconds(10f);
        gazePointVisualizer.ToggleGazeDot(true);
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
        HomePageControl(
            true,
            "請問您今天想進行哪一項眼動測試呢？\n（凝視選項5秒）",
            ""
        );
    }

    public void EndTest()
    {
        endTime = Time.time;
        testDuration = endTime - startTime;
        int score;
        if (mode == "black")
        {
            if (testDuration <= 60) score = 2;
            else if (testDuration <= 73) score = 1;
            else score = 0;

        }
        else
        {
            if (testDuration <= 180) score = 2;
            else if (testDuration <= 236) score = 1;
            else score = 0;
        }
        ShowResult(score);
        startBtn.SetActive(false);
        TestPageControl(false);
    }
}
