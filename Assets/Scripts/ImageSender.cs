using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ImageSender : MonoBehaviour
{
    private string serverUrl = "http://YOUR_PC_IP:5000/process_image";

    public IEnumerator SendImageToServer(Texture2D texture)
    {
        byte[] imageBytes = texture.EncodeToPNG();
        
        string base64Image = System.Convert.ToBase64String(imageBytes);

        string jsonData = JsonUtility.ToJson(new ImageData { image = base64Image });

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Image sent successfully!");
                string response = request.downloadHandler.text;
                Debug.Log("Server response: " + response);
            }
            else
            {
                Debug.LogError("Error sending image: " + request.error);
            }
        }
    }
}

[System.Serializable]
public class ImageData
{
    public string image;
}
