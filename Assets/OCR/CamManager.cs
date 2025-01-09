using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Windows.WebCam;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using Debug = UnityEngine.Debug;


public class CamManager : MonoBehaviour
{
    [SerializeField] private GameObject PhotoPrefab;
    [SerializeField] private TextToSpeechManager ttsManager;
    [SerializeField] private ImageSender imageSender;  // Reference to ImageSender
    //[SerializeField] private 

    private PhotoCapture photoCaptureObject = null;
    private Texture2D lastCapturedTexture;
    private Resolution cameraResolution;
    private CameraParameters cameraParameters;

    void Awake()
    {
        if (PhotoPrefab != null)
        {
            PhotoPrefab.SetActive(false);
        }
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        Debug.Log("Initializing camera...");
        var resolutions = PhotoCapture.SupportedResolutions;
        if (resolutions == null || !resolutions.Any())
        {
            Debug.LogError("No supported resolutions found");
            return;
        }

        cameraResolution = resolutions.First();
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        cameraParameters = new CameraParameters
        {
            hologramOpacity = 0.0f,
            cameraResolutionWidth = cameraResolution.width,
            cameraResolutionHeight = cameraResolution.height,
            pixelFormat = CapturePixelFormat.BGRA32
        };
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Photo mode started successfully");
        }
        else
        {
            Debug.LogError("Unable to start photo mode");
        }
    }

    public void TakePicture()
    {
        InitializeCamera();
        photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
        {
            if (photoCaptureObject == null)
            {
                Debug.LogError("Photo Capture Object is null");
                return;
            }

            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            ttsManager.SpeakText("Taking picture");

        });
    }

    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // Create our Texture2D for use and set the correct resolution
            if (lastCapturedTexture == null || lastCapturedTexture.width != cameraResolution.width)
            {
                lastCapturedTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            }

            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(lastCapturedTexture);
            Debug.Log("Photo captured successfully");
            
            // Automatically process the captured image
            ReadOutText();
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.LogError("Failed to capture photo");
            ttsManager.SpeakText("Failed to capture photo");
        }
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown our photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    public void ReadOutText()
    {
        if (lastCapturedTexture == null)
        {
            ttsManager.SpeakText("No photo available to analyze");
            return;
        }
        try
        {
            StartCoroutine(imageSender.SendImageToServer(lastCapturedTexture));
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending image: {e.Message}");
            ttsManager.SpeakText("Error sending image for processing");
        }
    }

    public void DisplayOnSlate()
    {
        if (lastCapturedTexture == null)
        {
            Debug.LogWarning("No photo captured yet!");
            return;
        }


        PhotoPrefab.SetActive(true);

        // Set up the slate position
        Vector3 slatePosition = Camera.main.transform.position +
                              (Camera.main.transform.forward * 0.6f);
        PhotoPrefab.transform.position = slatePosition;
        PhotoPrefab.transform.rotation = Quaternion.LookRotation(
            Camera.main.transform.forward, Vector3.up);

        // Set the texture
        var rawImage = PhotoPrefab.GetComponentInChildren<RawImage>();
        if (rawImage != null)
        {
            rawImage.texture = lastCapturedTexture;
        }

        // Ensure ObjectManipulator is present and configured
        var objectManipulator = PhotoPrefab.GetComponent<ObjectManipulator>();
        if (objectManipulator == null)
        {
            objectManipulator = PhotoPrefab.AddComponent<ObjectManipulator>();
        }

        // Add bounds control for scaling
        var boundsControl = PhotoPrefab.GetComponent<BoundsControl>();
        if (boundsControl == null)
        {
            boundsControl = PhotoPrefab.AddComponent<BoundsControl>();
        }
    }

    void OnDisable()
    {
        if (photoCaptureObject != null)
        {
            photoCaptureObject.StopPhotoModeAsync(result => {
                photoCaptureObject.Dispose();
                photoCaptureObject = null;
            });
        }
    }
}