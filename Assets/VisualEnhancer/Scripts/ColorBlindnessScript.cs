using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ColorBlindnessScript : MonoBehaviour
{
    [SerializeField]
    GameObject nGo, prGO, dtGo, trGo;

    [SerializeField]
    GameObject webcamGO;


    [SerializeField]
    Material webcamMat;

    readonly string[] headers = { "Normal", "Protanopia", "Deuteranopia", "Tritanopia" };

    float intensity = 0;

    void Awake()
    {
        webcamMat.SetInteger("_Mode", 0);
    }

    public void ColorBtnTap(int btn){
        webcamMat.SetInteger("_Mode", btn);
    }
}
