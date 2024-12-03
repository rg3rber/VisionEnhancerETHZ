using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[System.Serializable]
public class OpenAIRequest
{
    public string model;
    public OpenAIMessage[] messages;
    public float temperature;
}

[System.Serializable]
public class OpenAIMessage
{
    public string role;
    public string content;
}

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

[System.Serializable]
public class GPTResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class Message
{
    public string content;
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
    [SerializeField] private TextToSpeechManager ttsManager;
    [SerializeField] private LoadingScreenManager loadingScreenManager;
    [SerializeField] private string serverUrl = "http://localhost:5000/process_image";
    private float processingStartTime;
    private readonly string OPENAI_API_KEY = "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";
    private readonly string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";

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
        processingStartTime = Time.time;
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
        string jsonData = JsonUtility.ToJson(new ImageRequestData { image = base64Image });

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending image to server...");
            yield return request.SendWebRequest();

            if (loadingScreenManager != null)
            {
                loadingScreenManager.HideLoadingScreen();
            }

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

    private async void ProcessServerResponse(string response)
    {
        float processingTime = Time.time - processingStartTime;
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
                    string processedText = await FilterMathematicalContent(detectedText);
                    Debug.Log("Text processed by ChatGPT");
                    Debug.Log($"Detected text (processed in {processingTime:F2} seconds): {processedText}");
                    ttsManager.SpeakText(processedText);
                }
            }
        }
        catch (System.Exception e)
        {
            string errorMessage = $"Error processing response: {e.Message}";
            Debug.LogError(errorMessage);
            ttsManager.SpeakText(errorMessage);
        }
    }

    private async Task<string> FilterMathematicalContent(string text)
    {
        Debug.Log("Original text before GPT processing: " + text);
        string prompt = @"You are a text processor. Your task is to:
1. Analyze the input text
2. If the text contains more than 2 mathematical symbols, equations, or variables, respond with ONLY: 'This appears to be a mathematical formula'
3. Otherwise, remove ALL mathematical content including:
   - Variables (single letters, Greek letters)
   - Numbers with subscripts or superscripts
   - Mathematical operators (+, -, ×, ÷, =, etc.)
   - Any sequences that look like formulas
4. Keep only plain English descriptive text
5. Do not preserve any part of equations or formulas

6. Remove:
- URLs and email addresses
   - References and citations (e.g., '[1]', 'et al.', 'Figure 3.2')
   - Code snippets or programming syntax
   - Table data and numerical lists
   - Slide numbers or page numbers
   - Lengthy parenthetical asides
   - File paths or technical specifications
7. If the text contains more than 3 instances of the content in point 6, respond with ONLY: 'This content contains technical information not suitable for speech output'

Input text: " + text;

        using (UnityWebRequest request = new UnityWebRequest(OPENAI_API_URL, "POST"))
        {
            var requestData = new OpenAIRequest
            {
                model = "gpt-3.5-turbo",
                messages = new OpenAIMessage[]
                {
                    new OpenAIMessage { role = "system", content = "You are a specialized assistant that identifies mathematical content. If you see more than 2 mathematical elements, respond with 'This appears to be a mathematical formula'. Otherwise, remove ALL mathematical content completely." },
                    new OpenAIMessage { role = "user", content = prompt }
                },
                temperature = 0.3f
            };

            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log("Sending to GPT: " + jsonData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {OPENAI_API_KEY}");

            var operation = request.SendWebRequest();
            await operation;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GPT API Error: {request.error}");
                return text;
            }

            var gptResponse = JsonUtility.FromJson<GPTResponse>(request.downloadHandler.text);
            return gptResponse.choices[0].message.content;
        }
    }
}