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
    public GameObject crossHairUI;

    private UIIndex currentUIIndex = UIIndex.Inventory;

    private enum UIIndex
    {
        Map,
        Quest,
        Inventory,
        Setting
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
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crossHairUI.SetActive(true);
        }
    }
}