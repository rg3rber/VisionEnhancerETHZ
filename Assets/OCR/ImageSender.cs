using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TMPro;

[System.Serializable]
public class ImageRequestData
{
    public string image;
}

[System.Serializable]
public class ServerResponse
{
    public bool success;
    public string detected_text;
    public string error;
}

public static class UnityWebRequestExtensions
{
    public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation operation)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest.Result>();
        operation.completed += asyncOp => tcs.SetResult(((UnityWebRequestAsyncOperation)asyncOp).webRequest.result);
        return tcs.Task.GetAwaiter();
    }
}

public class ImageSender : MonoBehaviour
{
    [SerializeField] private GameObject webcamObj;
    [SerializeField] private TextToSpeechManager ttsManager;
    [SerializeField] private TMP_InputField iptext;
    [SerializeField] private GameObject ipObj;
    [SerializeField] private GameObject hideBtn;
    // [SerializeField] private string serverUrl = "http://localhost:5000/process_image";
    private float processingStartTime;

    private string GOOGLE_VISION_API_KEY = "AIzaSyBvlLMD5Ys9MJ_3zkEr9CfNGuTYZ7NB2RQ";
    private string GOOGLE_VISION_API_URL = "https://vision.googleapis.com/v1/images:annotate";


    public void ToggleWebcam() {
        webcamObj.SetActive(!webcamObj.activeSelf);
    }

    public void ToggleSettings() {
        ipObj.SetActive(!ipObj.activeSelf);
        hideBtn.SetActive(!hideBtn.activeSelf);
    }

    private void Start()
    {
        if (ttsManager == null)
        {
            Debug.LogError("TextToSpeechManager reference not set in ImageSender");
        }
        // if (loadingScreenManager == null)
        // {
        //     Debug.LogError("LoadingScreenManager reference not set in ImageSender");
        // }
    }

    public IEnumerator DirectSendImage(Texture2D texture) {
        processingStartTime = Time.time;
        if (texture == null)
        {
            Debug.LogError("Texture is null in SendImageToServer");
            yield break;
        }

        ttsManager?.SpeakText("Processing image");
        byte[] imageBytes = texture.EncodeToPNG();
        string base64Image = System.Convert.ToBase64String(imageBytes);
        string jsonData = JsonUtility.ToJson(new ImageRequestData { image = base64Image });

        string greq = "{\"requests\": [{\"features\": [{\"type\":\"TEXT_DETECTION\"}], \"image\":{\"content\": \"" + base64Image + "\"}}]}";

        Debug.Log(greq);

        // send to google vision (ignore chatgpt stuff for now)
        using (UnityWebRequest request = new UnityWebRequest(GOOGLE_VISION_API_URL + "?key=" + GOOGLE_VISION_API_KEY, "POST")) {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(greq);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            Debug.Log("Sending image to server...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Image sent successfully!");
                string response = request.downloadHandler.text;
                Debug.Log(response);
                Regex regex = new Regex("\"description\": \"(.*)\"");
                Match m = regex.Match(response);
                if (m.Success) {
                    ttsManager?.SpeakText(m.Groups[1].ToString());
                } else {
                    ttsManager?.SpeakText("No text found");
                }
                // Debug.Log(regex.Match(response).Groups[1]);
                // if (Regex.M)
                // ProcessServerResponse(response);
            }
            else
            {
                string errorMessage = $"Error sending image: {request.error}";
                Debug.LogError(errorMessage);
                ttsManager?.SpeakText(errorMessage);
            }

        }

    }

    public IEnumerator SendImageToServer(Texture2D texture)
    {
        processingStartTime = Time.time;
        if (texture == null)
        {
            Debug.LogError("Texture is null in SendImageToServer");
            yield break;
        }

        // if (loadingScreenManager != null)
        // {
        //     loadingScreenManager.ShowLoadingScreen();
        // }

        ttsManager?.SpeakText("Processing image");

        byte[] imageBytes = texture.EncodeToPNG();
        string base64Image = System.Convert.ToBase64String(imageBytes);
        string jsonData = JsonUtility.ToJson(new ImageRequestData { image = base64Image });

        Debug.Log("http://" + iptext.text + "/process_image");

        using (UnityWebRequest request = new UnityWebRequest("http://" + iptext.text + "/process_image", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending image to server...");
            yield return request.SendWebRequest();

            // if (loadingScreenManager != null)
            // {
            //     loadingScreenManager.HideLoadingScreen();
            // }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Image sent successfully!");
                string response = request.downloadHandler.text;
                ProcessServerResponse(response);
            }
            else
            {
                string errorMessage = $"Error sending image: {request.error}";
                Debug.LogError(errorMessage);
                ttsManager?.SpeakText(errorMessage);
            }
        }
    }

    private async void ProcessServerResponse(string response)
    {
        float processingTime = Time.time - processingStartTime;
        try
        {
            var jsonResponse = JsonUtility.FromJson<ServerResponse>(response);
            if (jsonResponse.success)
            {
                string processedText = jsonResponse.detected_text;
                if (string.IsNullOrEmpty(processedText))
                {
                    ttsManager?.SpeakText("No text was detected in the image");
                }
                else
                {
                    Debug.Log($"Processed text ({processingTime:F2} seconds): {processedText}");
                    ttsManager?.SpeakText(processedText);
                }
            }
            else
            {
                string errorMessage = $"Server error: {jsonResponse.error}";
                Debug.LogError(errorMessage);
                ttsManager?.SpeakText(errorMessage);
            }
        }
        catch (System.Exception e)
        {
            string errorMessage = $"Error processing response: {e.Message}";
            Debug.LogError(errorMessage);
            ttsManager?.SpeakText(errorMessage);
        }
    }
}