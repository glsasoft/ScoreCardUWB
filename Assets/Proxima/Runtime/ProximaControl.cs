using Proxima;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximaControl : MonoBehaviour
{
    public float limitTime = 2.0f;
    public int limitClickCount = 6;
    public GameObject proximaInspector;

    float clickTime = 0;
    int clickCount = 0;
    float gridWidth = 0;
    Vector3 mousePos;

    private void Awake()
    {
        if (proximaInspector)
            proximaInspector.SetActive(false);

        DontDestroyOnLoad(gameObject);

        gridWidth = Screen.width / 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (clickTime != 0)
        {
            if (Time.time - clickTime > limitTime)
            {
                clickTime = 0;
                clickCount = 0;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(clickTime == 0)
                clickTime = Time.time;

            mousePos = Input.mousePosition;

            if(clickCount % 3 == 0 && mousePos.x > 0 && mousePos.x <= gridWidth)
                clickCount++;
            else if (clickCount % 3 == 1 && mousePos.x > gridWidth && mousePos.x <= gridWidth * 2)
                clickCount++;
            else if (clickCount % 3 == 2 && mousePos.x > gridWidth * 2 && mousePos.x <= gridWidth * 3)
                clickCount++;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if (clickCount >= limitClickCount && Time.time - clickTime <= limitTime)
            {
                if (proximaInspector)
                {
                    proximaInspector.SetActive(!proximaInspector.activeSelf);
                    if (proximaInspector.activeSelf)
                        Debug.Log("开启 Proxima Inspector");
                    else
                        Debug.Log("关闭 Proxima Inspector");
                }

                clickTime = 0;
                clickCount = 0;
            }
        }
    }
}
