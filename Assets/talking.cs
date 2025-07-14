using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class talking : MonoBehaviour
{
    public GameObject main_page;       // 要監控的物件
    public GameObject QA_page;
    public GameObject calibration_page;
    public GameObject exe_page1;
    public GameObject exe_page2;
    public GameObject exe_page3;
    public GameObject exe_page4;
    public GameObject exe_page5;
    public GameObject exe_page6;
    bool calibration_page_act = false;
    bool main_page_act = false;
    bool QA_page_act = false;
    bool exe_page1_act = false;
    bool exe_page2_act = false;
    bool exe_page3_act = false;
    bool exe_page4_act = false;
    bool exe_page5_act = false;
    bool exe_page6_act = false;
    bool QA_ing = false;
    string last_QA;
    public TextMeshProUGUI QA_text;
    int i = 2;

    public AudioClip[] voiceClip;         // 多個語音
    private AudioSource audioSource;

    private bool wasActive = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

    }


    void Update()
    {
        if (main_page == null) return;

        if (!calibration_page_act && calibration_page.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[0]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        calibration_page_act = calibration_page.activeSelf;


        if (!main_page_act && main_page.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[1]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        main_page_act = main_page.activeSelf;

        if (!QA_page_act && QA_page.activeSelf)
        {

            QA_ing = true;

        }
        QA_page_act = QA_page.activeSelf;
        if (QA_ing && (last_QA != QA_text.text))
        {
            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[i];
            }

            audioSource.Play();
            i++;
            if (i == 9)
            {
                QA_ing = false;
                i = 2;
            }

        }
        last_QA = QA_text.text;



        if (!exe_page1_act && exe_page1.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[9]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        exe_page1_act = exe_page1.activeSelf;

        if (!exe_page2_act && exe_page2.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[10]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        exe_page2_act = exe_page2.activeSelf;

        if (!exe_page3_act && exe_page3.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[11]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        exe_page3_act = exe_page3.activeSelf;

        if (!exe_page4_act && exe_page4.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[12]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        exe_page4_act = exe_page4.activeSelf;
        if (!exe_page5_act && exe_page5.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[13]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        exe_page5_act = exe_page5.activeSelf;
        if (!exe_page6_act && exe_page6.activeSelf)
        {



            if (voiceClip.Length > 1)
            {
                audioSource.clip = voiceClip[14]; // 例如第 1 段語音
            }

            audioSource.Play();

        }
        exe_page6_act = exe_page6.activeSelf;
        
    }
}
