using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ImageSender : MonoBehaviour
{
    [SerializeField] private TextToSpeechManager ttsManager;
    [SerializeField] private LoadingScreenManager loadingScreenManager;
    [SerializeField] private string serverUrl = "http://localhost:5000/process_image";

    private void Start()
    {
        if (ttsManager == null)
        {
            Debug.LogError("TextToSpeechManager reference not set in ImageSender");
        }
        if (loadingScreenManager == null)
        {
            Debug.LogError("LoadingScreenManager reference not set in ImageSender");
        }
    }

    public IEnumerator SendImageToServer(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null in SendImageToServer");
            yield break;
        }

        if (loadingScreenManager != null)
        {
            loadingScreenManager.ShowLoadingScreen();
        }

        if (ttsManager != null)
        {
            ttsManager.SpeakText("Processing image");
        }

        byte[] imageBytes = texture.EncodeToPNG();
        string base64Image = System.Convert.ToBase64String(imageBytes);

        string jsonData = JsonUtility.ToJson(new ImageData { image = base64Image });

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending image to server...");
            yield return request.SendWebRequest();

            loadingScreenManager.HideLoadingScreen();

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
                ttsManager.SpeakText(errorMessage);
            }
        }
    }

    private void ProcessServerResponse(string response)
    {
        try
        {
            var jsonResponse = JsonUtility.FromJson<ServerResponse>(response);
            if (jsonResponse.success)
            {
                string detectedText = jsonResponse.detected_text;
                if (string.IsNullOrEmpty(detectedText))
                {
                    ttsManager.SpeakText("No text was detected in the image");
                }
                else
                {
                    Debug.Log($"Detected text: {detectedText}");
                    ttsManager.SpeakText(detectedText);
                }
            }
            else
            {
                string errorMessage = $"Server error: {jsonResponse.error}";
                Debug.LogError(errorMessage);
                ttsManager.SpeakText(errorMessage);
            }
        }
        catch (System.Exception e)
        {
            string errorMessage = $"Error processing server response: {e.Message}";
            Debug.LogError(errorMessage);
            ttsManager.SpeakText(errorMessage);
        }
    }
}

[System.Serializable]
public class ImageData
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