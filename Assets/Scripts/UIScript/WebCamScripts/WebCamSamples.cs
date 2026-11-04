using System.Collections;
using System.Collections.Generic;
using LetsPiu.UI.UIScript.WebCamScripts.Logic;
using TMPro;
using UnityEngine;

public class WebCamSamples : MonoBehaviour
{
    public QrCodeScanWebCamLogic qrCodeScanWebCam;
    public TMP_Text scanResult;
    
    // Start is called before the first frame update
    void Start()
    {
        qrCodeScanWebCam.ScanResultEvent += result =>
        {
            scanResult.text = "二维码:"+result.Text;
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
