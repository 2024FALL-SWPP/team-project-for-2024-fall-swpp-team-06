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

    private int[] totalnums={3, 2, 2};
    private string[] texts={"", "", ""};

    // Start is called before the first frame update
    void Start()
    {
        detectionPanel.SetActive(true);
        detectionText.text = "";
        telenumPanel.SetActive(true);
        numText_1.text = "";
        numText_2.text = "";
        numText_3.text = "";
        
    }

    // Update is called once per frame
    void Update()
    {
        if (teledetector != null && teledetector.detected)
        {
            detectionText.text = "DETECTED!";
        }
        else
        {
            detectionText.text = "";
        }

        if (basemanager!=null)
        {   
            for(int i = 0; i < 3; i++)
            {
                if(basemanager.isBaseRegistered[i])
                {
                    switch(i)
                    {
                        case 0:
                            texts[i] = $"Region{i+1} : {telemanager.teleRegion1} / {totalnums[i]}";
                            break;
                        case 1:
                            texts[i] = $"Region{i+1} : {telemanager.teleRegion2} / {totalnums[i]}";
                            break;
                        case 2:
                            texts[i] = $"Region{i+1} : {telemanager.teleRegion3} / {totalnums[i]}";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    texts[i] = "*******";
                }
            }

            numText_1.text=texts[0];
            numText_2.text=texts[1];
            numText_3.text=texts[2];
        }
        else
        {
            numText_1.text = "";
            numText_2.text = "";
            numText_3.text = "";
        }
    }
}
