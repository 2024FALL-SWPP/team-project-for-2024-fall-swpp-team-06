using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public MinimapController minimapController;
    
    public bool isUIActive { get; private set; } = false;
    public QuestSystem questSystem;
    public GameObject settingUI;
    public GameObject inventoryUI;
    public GameObject crossHairUI;
    
    private UIIndex currentUIIndex;

    private enum UIIndex
    {
        Map, Quest, Inventory, Setting
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
                return;
            }
            
            ActivateUI(UIIndex.Setting);
            isUIActive = true;
        }
    }

    void ActivateUI(UIIndex uiIndex)
    {
        switch (uiIndex)
        {
            case UIIndex.Map:
                minimapController.ShowFullMap();
                break;
            case UIIndex.Quest:
                questSystem.ToggleActive();
                break;
            case UIIndex.Inventory:
                inventoryUI.SetActive(true);
                break;
            default:
                settingUI.SetActive(true);
                break;
        }
    }

    void DeactivateUI(UIIndex uiIndex)
    {
        switch (uiIndex)
        {
            case UIIndex.Map:
                minimapController.CloseFullMap();
                break;
            case UIIndex.Quest:
                questSystem.ToggleActive();
                break;
            case UIIndex.Inventory:
                inventoryUI.SetActive(false);
                break;
            default:
                settingUI.SetActive(false);
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
        }
        else
        {
            isUIActive = true;
            UpdateUIState();
        }
                
        if (currentUIIndex != uiIndex)
        {
            ActivateUI(uiIndex);
            currentUIIndex = uiIndex;
        }
        else
        {
            isUIActive = false;
            UpdateUIState();
        }
    }
    
    private void UpdateUIState()
    {
        if (isUIActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            crossHairUI.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crossHairUI.SetActive(true);
        }
    }
}
