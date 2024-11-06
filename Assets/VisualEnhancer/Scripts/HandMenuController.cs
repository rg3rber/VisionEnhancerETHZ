using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HandMenuController : MonoBehaviour
{
    [SerializeField]
    GameObject headerGO, backGO, zoomGO, incGO, decGO, prGO, dtGo, trGo;

    [SerializeField]
    GameObject webcamGO;

    [SerializeField]
    TMP_Text menu_header, inc_text, dec_text;

    [SerializeField]
    Material webcamMat;

    readonly string[] headers = { "Zoom", "Protanopia", "Deuteranopia", "Tritanopia" };

    float intensity = 0;

    void Awake()
    {
        webcamMat.SetFloat("_Zoom", 0);
        webcamMat.SetInteger("_Mode", 0);
    }

    public void BtnTap(int btn)
    {
        // enable header
        headerGO.SetActive(true);

        // enable the webcam filter
        webcamGO.SetActive(true);

        // disable all other buttons
        zoomGO.SetActive(false);
        prGO.SetActive(false);
        dtGo.SetActive(false);
        trGo.SetActive(false);

        // enable the back, inc and dec
        backGO.SetActive(true);
        incGO.SetActive(true);
        decGO.SetActive(true);

        menu_header.text = headers[btn];

        webcamMat.SetInteger("_Mode", btn);

        if (btn == 0)
        {
            inc_text.text = "Zoom In";
            dec_text.text = "Zoom Out";
            intensity = 0;
        } else {
            inc_text.text = "Increase intensity";
            dec_text.text = "Decrease intensity";
            intensity = 0.5f;
        }
        webcamMat.SetFloat("_Zoom", intensity);
    }

    public void BackTapped()
    {
        // disable header
        headerGO.SetActive(false);

        // disable webcam filter
        webcamGO.SetActive(false);

        // re-enable filter buttons
        zoomGO.SetActive(true);
        prGO.SetActive(true);
        dtGo.SetActive(true);
        trGo.SetActive(true);

        // disable navi btns
        backGO.SetActive(false);
        incGO.SetActive(false);
        decGO.SetActive(false);
    }

    public void IncreaseTapped()
    {
        intensity = Mathf.Clamp(intensity + 0.1f, 0, 1);
        webcamMat.SetFloat("_Zoom", intensity);
    }

    public void DecreaseTapped()
    {
        intensity = Mathf.Clamp(intensity - 0.1f, 0, 1);
        webcamMat.SetFloat("_Zoom", intensity);
    }
}
