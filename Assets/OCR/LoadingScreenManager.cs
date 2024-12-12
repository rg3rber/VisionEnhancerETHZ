using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreenCanvas;

    public void ShowLoadingScreen()
    {
        if (loadingScreenCanvas != null)
        {
            loadingScreenCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Loading screen canvas reference is missing");
        }
    }

    public void HideLoadingScreen()
    {
        if (loadingScreenCanvas != null)
        {
            loadingScreenCanvas.SetActive(false);
        }
    }
} 