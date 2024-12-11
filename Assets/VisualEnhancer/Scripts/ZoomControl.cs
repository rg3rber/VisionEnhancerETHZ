using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class ZoomControl : MonoBehaviour
{
    [SerializeField]
    GameObject incGo, decGo;

    [SerializeField]
    GameObject webcamGO;

    [SerializeField]
    Material webcamMat;

    // [SerializeField]
    // Slider zoomSlider;
    // Start is called before the first frame update

    float intensity = 0.0f;

    void Awake(){
        webcamMat.SetFloat("_Zoom", 0);
    }

    public void ZoomIncrease(){
        intensity = Mathf.Clamp(intensity + 0.1f, 0, 1); 
        webcamMat.SetFloat("_Zoom", intensity);
    }

    public void ZoomDecrease(){
        intensity = Mathf.Clamp(intensity - 0.1f, 0, 1);
        webcamMat.SetFloat("_Zoom", intensity);
    }

    // public void ZoomSet() {
    //     // Debug.Log(zoomSlider.value);
    //     // intensity = Mathf.Clamp( - 0.1f, 0, 1);
    //     // webcamMat.SetFloat("_Zoom", intensity);
    // }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
