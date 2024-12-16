using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUIManager : MonoBehaviour
{
    public CameraRaycaster cameraRaycaster;
    
    private Image mouseImage;
    private TextMeshProUGUI interactionText;
    
    private int clickableLayerIndex = 3;
    
    // Start is called before the first frame update
    void Start()
    {
        mouseImage = transform.GetChild(0).GetComponent<Image>();
        interactionText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        cameraRaycaster.OnRaycastHit += ProcessRaycast;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ProcessRaycast(bool isHit, RaycastHit hit, bool isClicked)
    {
        if (!isHit)
        {
            mouseImage.enabled = false;
            interactionText.enabled = false;
            return;
        }

        if (hit.collider.gameObject.layer == clickableLayerIndex)
        {
            if (hit.collider.gameObject.tag.StartsWith("Base"))
            {
                interactionText.text = "Register Base";
            }
            else
            {
                interactionText.text = "Interact";
            }
            
            mouseImage.enabled = true;
            interactionText.enabled = true;
        }
        else
        {
            mouseImage.enabled = false;
            interactionText.enabled = false;
        }
    }
}
