using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ColorBlindnessScript : MonoBehaviour
{
    [SerializeField]
    GameObject headerGo, nGo, prGO, dtGo, trGo;

    [SerializeField]
    GameObject webcamGO;

    //[SerializeField]
    //TMP_Text menu_header, inc_text, dec_text;

    [SerializeField]
    Material webcamMat;

    readonly string[] headers = { "Normal", "Protanopia", "Deuteranopia", "Tritanopia" };

    float intensity = 0;

    void Awake()
    {
        webcamMat.SetInteger("_Mode", 0);
    }

    public void ColorBtnTap(int btn){
        //webcamGO.SetActive(true);

        nGo.SetActive(false);
        prGO.SetActive(false);
        dtGo.SetActive(false);
        trGo.SetActive(false);

        //menu_header.text = headers[btn];

        webcamMat.SetInteger("_Mode", btn);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
