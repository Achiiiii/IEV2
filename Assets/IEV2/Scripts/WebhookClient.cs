using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking;

[Serializable]
public class TestData
{
    public string pose_data; 
}

public class WebhookClient : MonoBehaviour
{
    // private TcpClient client;
    // private NetworkStream stream;
    // private string serverIP = "127.0.0.1"; // 
    // private int port = 2000; // 
    // // private VideoPoseTest scriptAReference;
    // string least_text = "";
    // string serverUrl = "http://localhost:5000/";
    // bool isConnected = false;
    // public bool calibration_suss = false;
    void Start()
    {
        // StartCoroutine(CallStartTask());
        // scriptAReference = GetComponent<VideoPoseTest>();
        // if (scriptAReference == null)Debug.LogWarning("找不到 VideoPoseTest 腳本！");
    }
//     IEnumerator Senddata(string send_text)
//     {

//         string jsonPayload = "{\"pose_data\": " + send_text + "}";

//         using (UnityWebRequest www = new UnityWebRequest(serverUrl + "update", "POST"))
//         {
//             byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
//             www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogWarning(www.result);
//             }
//         }
//     }
//     IEnumerator CallStartTask()
//     {

//         UnityWebRequest request = UnityWebRequest.PostWwwForm(serverUrl + "start", "");

//         // 發送請求
//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.Success)
//         {
//             Debug.Log("任務啟動成功：" + request.downloadHandler.text);
//         }
//         else
//         {
//             Debug.LogWarning("任務啟動失敗: " + request.responseCode + "\n" + request.error);
//         }
//     }
//     public bool IsJsonValid(string jsonString)
//     {
//         try
//         {
//             JsonUtility.FromJson<TestData>(jsonString);
//             return true;
//         }
//         catch (Exception e)
//         {
//             Debug.LogWarning("JSON 解析錯誤: " + e.Message);
//             return false;
//         }
//     }
//     void Sendpose(string message)
//     {
//         try
//         {
//             if (isConnected && stream != null && stream.CanWrite)
//             {
//                 byte[] data = Encoding.UTF8.GetBytes(message);
//                 stream.Write(data, 0, data.Length);
//                 //Debug.Log("Message sent: " + message);}
//             }
//         }
//         catch
//         {
//             Debug.Log("SendData Error.");
//             isConnected = false;

//         }
//     }
//     void TCPend()
//     {


//         byte[] data = Encoding.UTF8.GetBytes("end");
//         stream.Write(data, 0, data.Length);
 
//         // catch
//         // {
//         //     Debug.Log("SendData Error.");
//         //     isConnected = false;

//         // }

//     }
//     public void SendEnd()
//     {
//         TCPend();
//         StartCoroutine(EndSignal());
        
//     }
//     IEnumerator EndSignal()
//     {
//         UnityWebRequest request = UnityWebRequest.Get(serverUrl + "end");

//         // 發送請求
//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.Success)
//         {
//             Debug.Log("你的分數是:  " + request.downloadHandler.text);
//         }
//         else
//         {
//             Debug.LogWarning("任務啟動失敗: " + request.responseCode + "\n" + request.error);
//         }
//     }


//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Q) || calibration_suss == true)
//         {
//             // StartCoroutine(CallStartTask());
//             if (!isConnected)
//             {
//                 try
//                 {
//                     client = new TcpClient(serverIP, port);
//                     stream = client.GetStream();
//                     isConnected = true;
//                 }
//                 catch (Exception e)
//                 {
//                     Debug.Log("Error: " + e.Message);
//                 }
//             }
//             calibration_suss = false;

//         }
//         string pose_text = scriptAReference.myText;
//         if (isConnected)
//         {


//             if (pose_text != "" && pose_text != "[]" && pose_text != least_text)
//             {
//                 string jsonPayload = "{\"pose_data\": " + pose_text + "}";
//                 if (IsJsonValid(jsonPayload))
//                 {
//                     Sendpose(jsonPayload);
//                     least_text = pose_text;
//                 }
//                 // StartCoroutine(Senddata(pose_text));
//                 // least_text = pose_text;

//                 }
//         }
//     }
//         void OnApplicationQuit()
//     {
//         if (stream != null)
//             stream.Close();
//         if (client != null)
//             client.Close();
//     }
}
