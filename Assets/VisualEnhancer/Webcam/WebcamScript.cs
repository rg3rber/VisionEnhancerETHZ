using System;
using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class WebcamScript : MonoBehaviour
{
    WebCamTexture tex;

    [SerializeField]
    Material webcamMat;

    // [SerializeField]
    // Slider zoom, offx, offy;

    void Awake()
    {
        WebCamDevice device = WebCamTexture.devices[0];
        tex = new WebCamTexture(device.name);

        webcamMat.SetTexture("_MainTex", tex);
        tex.Play();
    }

    // void Update()
    // {
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
