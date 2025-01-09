using System;
using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.UI;
using MixedReality.Toolkit.SpatialManipulation;


public class WebcamScript : MonoBehaviour
{
    WebCamTexture tex;

    [SerializeField]
    Material webcamMat;

    [SerializeField] private GameObject PhotoPrefab;
    [SerializeField] private TextToSpeechManager ttsManager;
    [SerializeField] private ImageSender imageSender;  // Reference to ImageSender

    private Texture2D lastCapturedTexture;

    void Awake()
    {
        WebCamDevice device = WebCamTexture.devices[0];
        tex = new WebCamTexture(device.name);

        webcamMat.SetTexture("_MainTex", tex);
        tex.Play();

        PhotoPrefab?.SetActive(false);

    }

    public void ResetCamera() {
        WebCamDevice device = WebCamTexture.devices[0];
        tex = new WebCamTexture(device.name);

        webcamMat.SetTexture("_MainTex", tex);
        tex.Play();
    }

    public void TakePhoto() {
        // tex.
        lastCapturedTexture = new Texture2D(tex.width, tex.height);
        // IntPtr ptr = tex.GetNativeTexturePtr();
        // lastCapturedTexture.UpdateExternalTexture(ptr);
        lastCapturedTexture.SetPixels32(tex.GetPixels32());
        lastCapturedTexture.Apply();
        ReadOutText();
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
            // StartCoroutine(imageSender.DirectSendImage(lastCapturedTexture));
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

    //     // get zoom val from zoom slider
    //     float zoomVal = zoom.Value;
    //     // set zoom val in webcamMat
    //     webcamMat.SetFloat("_Zoom", zoomVal);

    //     // set max/min of offsetXY to +- zoom/2
    //     offx.MinValue = -zoomVal / 2;
    //     offy.MinValue = -zoomVal / 2;
    //     offx.MaxValue = zoomVal / 2;
    //     offy.MaxValue = zoomVal / 2;

    //     // clamp value
    //     offx.Value = Math.Clamp(offx.Value, -zoomVal / 2, zoomVal / 2);
    //     offy.Value = Math.Clamp(offy.Value, -zoomVal / 2, zoomVal / 2);

    //     // get offsetXY val from zoom sliders
    //     Vector2 offset = new Vector2(offx.Value, offy.Value);
    //     // set offsetXY val in webcamMat
    //     webcamMat.SetVector("_Offset", offset);
    // }
}
