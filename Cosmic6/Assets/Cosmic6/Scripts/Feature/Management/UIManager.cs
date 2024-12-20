using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using DevionGames.UIWidgets;

public class UIManager : MonoBehaviour
{

    public MinimapController minimapController;

    public bool isUIActive { get; private set; } = true;
    public QuestSystem questSystem;
    public UIWidget inventoryWidget;
    public UIWidget menuWidget;
    public GameObject triggerTooltip;
    public GameObject crossHairUI;
    public GameObject telePanel;
    public GameObject detectionPanel;
    public GameObject playerStatusUI;
    public GameObject itemSlot;
    public GameObject helmetImage;
    public GameObject mouseImage;
    public GameObject hoeImage;

    private GameObject[] itemSlotChildren;

    private UIIndex currentUIIndex = UIIndex.Inventory;

    private enum UIIndex
    {
        Map,
        Quest,
        Inventory,
        Setting
    }

    void Start()
    {
        var childCount = itemSlot.transform.childCount;
        itemSlotChildren = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            itemSlotChildren[i] = itemSlot.transform.GetChild(i).gameObject;
        }
        
        isUIActive = false;
        UpdateUIState();
        inventoryWidget.Close();
    }
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ProcessNormalUIInput(UIIndex.Map);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            ProcessNormalUIInput(UIIndex.Quest);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            ProcessNormalUIInput(UIIndex.Inventory);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isUIActive)
            {
                DeactivateUI(currentUIIndex);
                isUIActive = false;

                if (currentUIIndex != UIIndex.Setting)
                {
                    ActivateUI(UIIndex.Setting);
                    currentUIIndex = UIIndex.Setting;
                    isUIActive = true;
                }
            }
            else
            {
                ActivateUI(UIIndex.Setting);
                currentUIIndex = UIIndex.Setting;
                isUIActive = true;
            }
        }

        UpdateUIState();
    }

    void ActivateUI(UIIndex uiIndex)
    {
        switch (uiIndex)
        {
            case UIIndex.Map:
                minimapController.ShowFullMap();
                ToggleUIForMap(false);
                break;
            case UIIndex.Quest:
                questSystem.ToggleActive();
                break;
            case UIIndex.Inventory:
                inventoryWidget.Show();
                break;
            default:
                menuWidget.Toggle();
                break;
        }
    }

    void DeactivateUI(UIIndex uiIndex)
    {
        switch (uiIndex)
        {
            case UIIndex.Map:
                minimapController.CloseFullMap();
                ToggleUIForMap(true);
                break;
            case UIIndex.Quest:
                questSystem.ToggleActive();
                break;
            case UIIndex.Inventory:
                inventoryWidget.Close();
                break;
            default:
                menuWidget.Toggle();
                break;
        }
    }

    private void ProcessNormalUIInput(UIIndex uiIndex)
    {
        if (isUIActive)
        {
            if (currentUIIndex == UIIndex.Setting)
            {
                return;
            }

            DeactivateUI(currentUIIndex);
            
            if (currentUIIndex != uiIndex)
            {
                ActivateUI(uiIndex);
                currentUIIndex = uiIndex;
            }
            else
            {
                isUIActive = false;
            }
        }
        else
        {
            isUIActive = true;
            ActivateUI(uiIndex);
            currentUIIndex = uiIndex;
            
        }
    }

    private void UpdateUIState()
    {
        if (isUIActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            crossHairUI.SetActive(false);
            mouseImage.SetActive(false);
            hoeImage.SetActive(false);

            
            triggerTooltip.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crossHairUI.SetActive(true);
            mouseImage.SetActive(true);
            hoeImage.SetActive(true);
            
            triggerTooltip.SetActive(true);
        }
    }

    private void ToggleUIForMap(bool isOn)
    {
        telePanel.SetActive(isOn);
        detectionPanel.SetActive(isOn);
        playerStatusUI.SetActive(isOn);
        helmetImage.SetActive(isOn);

        foreach (var child in itemSlotChildren)
        {
            child.SetActive(isOn);
        }
    }
    
}