using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEngine.Windows.WebCam;

public class PhotoCamera : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;

    public GameObject PhotoPrefab;

    Resolution cameraResolution;

    float ratio = 1.0f;
    AudioSource shutterSound;

    // Use this for initialization
    void Start()
    {
        shutterSound = GetComponent<AudioSource>() as AudioSource;
        Debug.Log("File path " + Application.persistentDataPath);
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        ratio = (float)cameraResolution.height / (float)cameraResolution.width;

        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            Debug.Log("camera ready to take picture");
        });
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;
        Debug.Log("camera ready to take picture");

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }
    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }
    public void StopCamera()
    {
        // Deactivate our camera
        Debug.Log("stopping Camera");

        photoCaptureObject?.StopPhotoModeAsync(OnStoppedPhotoMode);
    }
    public void TakePicture()
    {
        // hack for now: first click = setup camera, second click = take picture
        if(photoCaptureObject == null)
        {
            // Create a PhotoCapture object
            PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
                photoCaptureObject = captureObject;
                Debug.Log("setup new photocapture");
            });
            return;
        }
        Debug.Log("Taking a Picture");
        CameraParameters cameraParameters = new CameraParameters();
        cameraParameters.hologramOpacity = 0.0f;
        cameraParameters.cameraResolutionWidth = cameraResolution.width;
        cameraParameters.cameraResolutionHeight = cameraResolution.height;
        cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

        // Activate the camera
        if (photoCaptureObject != null)
        {
            if (shutterSound != null)
            {
                shutterSound.Play();
            }
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                // Take a picture
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            });
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
       if (!result.success)
        {
            Debug.LogError("Failed to capture photo");
            return;
        }
        Debug.Log("1. Creating texture");
        // Copy the raw image data into our target texture
        var targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // for OCR later
        /*
        byte[] imageBytes = targetTexture.EncodeToPNG();
        StartCoroutine(GetComponent<ApiManager>().GoogleRequest(imageBytes));
        */

        // Create a gameobject that we can apply our texture to
        Debug.Log("2. Instantiating slate");
        GameObject newElement = Instantiate<GameObject>(PhotoPrefab);
        GameObject imageHolder = newElement.transform.Find("PhotoContent/Canvas/RawImage").gameObject;
        Debug.Log("GameObject imageholder = " + imageHolder);

        imageHolder.GetComponent<RawImage>().texture = targetTexture;

        //Material newMaterial = new Material(Shader.Find("Unlit/Texture"));
        //Debug.Log("newMaterial = " + newMaterial);
        //newMaterial.mainTexture = targetTexture;
        //slateRenderer.material = newMaterial;
        // new Material(Shader.Find("Unlit/Texture"));

        // Set position and rotation 
        // Bug in Hololens v2 and Unity 2019 about PhotoCaptureFrame not having the location data - March 2020
        // 
        // Matrix4x4 cameraToWorldMatrix;
        // photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
        //  Vector3 position = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);
        //  Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));
        // Vector3 cameraForward = cameraToWorldMatrix * Vector3.forward;
        Debug.Log("7. Positioning slate");
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.Normalize();
        newElement.transform.position = Camera.main.transform.position + (cameraForward * 0.6f);

        newElement.transform.rotation = Quaternion.LookRotation(cameraForward, Vector3.up);
        Vector3 scale = newElement.transform.localScale;
        scale.y = scale.y * ratio; // scale the entire photo on height
        newElement.transform.localScale = scale;
        Debug.Log("Photo placed on slate successfully");

        // Clean up
        StopCamera();
    }
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("shutting down camera");
        // Shutdown our photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}