using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class TeleUI : MonoBehaviour
{
    public TeleDetector teledetector;
    public BaseManager basemanager;
    public TeleManager telemanager;
    public GameObject detectionPanel;
    public GameObject telenumPanel;
    public TMP_Text detectionText;
    public TMP_Text numText_1;
    public TMP_Text numText_2;
    public TMP_Text numText_3;
    public GameObject[] numTextObjects;
    private TMP_Text[] numTexts;

    private int[] totalnums={3, 2, 2};
    private string[] texts={"", "", ""};

    // Start is called before the first frame update
    void Start()
    {
        detectionPanel.SetActive(true);
        detectionText.text = "";
        telenumPanel.SetActive(true);
        
        numTexts = new TMP_Text[numTextObjects.Length];

        for (int i = 0; i < numTextObjects.Length; i++)
        {
            numTexts[i] = numTextObjects[i].GetComponent<TMP_Text>();
            if (i != 0)
            {
                numTextObjects[i].SetActive(false);
            }
        }
        UpdateTeleProgress();
    }

    // Update is called once per frame
    void Update()
    {
        if (teledetector != null && teledetector.detected)
        {
            detectionText.text = "감지됨!";
            detectionText.color = Color.red;
        }
        else
        {
            detectionText.text = "작동 중...";
            detectionText.color = Color.white;
        }
    }

    public void UpdateTeleProgress()
    {
        for(int i = 0; i < 3; i++)
        {
            if(basemanager.isBaseRegistered[i])
            {
                numTexts[i].fontSize = 25;
                numTextObjects[i].SetActive(true);
                texts[i] = $"지역{i+1} - {telemanager.teleRegion1}/{totalnums[i]}";
            }
            else
            {
                texts[i] = "*******";
            }
            
            numTexts[i].text=texts[i];
        }
    }
}
