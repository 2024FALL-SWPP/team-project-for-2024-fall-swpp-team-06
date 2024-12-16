using UnityEngine;
using System;
using DevionGames.UIWidgets;

public class UIManager : MonoBehaviour
{
    public UIWidget inventoryWidget;
    public GameObject questPanel;
    public GameObject minimapCamera;
    public GameObject fullmapCamera;
    public GameObject minimapCanvas;

    public MinimapController minimapController; // MinimapController 참조

    private bool inventoryOn = false;
    private bool questOn = false;
    private bool fullOn = false; 

    private string previousState = "IQM 000";

    void Awake()
    {
        if (inventoryWidget != null)
        {
            WidgetInputHandler.UnregisterInput(KeyCode.I, inventoryWidget);
        }
    }

    void Start()
    {
        if (minimapCamera != null) minimapCamera.SetActive(true);
        if (fullmapCamera != null) fullmapCamera.SetActive(false);
        if (questPanel != null) questPanel.SetActive(false);

        inventoryOn = false;
        questOn = false;
        fullOn = false;

        UpdateMinimapCanvasState();
        LogStateChange("Start");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuest();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMinimapMode();
        }
    }

    public void CloseInventoryFromButton()
    {
        if (inventoryOn && inventoryWidget != null)
        {
            inventoryWidget.Close();
            inventoryOn = false;
            LogStateChange("CloseInventoryFromButton");
        }
    }

    private void ToggleInventory()
    {
        if (!inventoryOn)
        {
            if (questOn)
            {
                questPanel.SetActive(false);
                questOn = false;
            }

            if (fullOn)
            {
                // full->partial
                fullOn = false;
                fullmapCamera.SetActive(false);
                minimapCamera.SetActive(true);
                if (minimapController != null)
                    minimapController.CloseFullMap();
            }

            if (inventoryWidget != null) inventoryWidget.Show();
            inventoryOn = true;
        }
        else
        {
            if (inventoryWidget != null) inventoryWidget.Close();
            inventoryOn = false;
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleInventory");
    }

    private void ToggleQuest()
    {
        if (!questOn)
        {
            if (inventoryOn)
            {
                inventoryWidget.Close();
                inventoryOn = false;
            }

            if (fullOn)
            {
                // full->partial
                fullOn = false;
                fullmapCamera.SetActive(false);
                minimapCamera.SetActive(true);
                if (minimapController != null)
                    minimapController.CloseFullMap();
            }

            questPanel.SetActive(true);
            questOn = true;
        }
        else
        {
            questPanel.SetActive(false);
            questOn = false;
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleQuest");
    }

    private void ToggleMinimapMode()
    {
        if (inventoryOn)
        {
            inventoryWidget.Close();
            inventoryOn = false;
        }

        if (questOn)
        {
            questPanel.SetActive(false);
            questOn = false;
        }

        if (!fullOn)
        {
            // partial->full
            fullOn = true;
            minimapCamera.SetActive(false);
            fullmapCamera.SetActive(true);

            if (minimapController != null)
                minimapController.ShowFullMap();
        }
        else
        {
            // full->partial
            fullOn = false;
            fullmapCamera.SetActive(false);
            minimapCamera.SetActive(true);

            if (minimapController != null)
                minimapController.CloseFullMap();
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleMinimapMode");
    }

    private void UpdateMinimapCanvasState()
    {
        if (minimapCanvas != null)
        {
            minimapCanvas.SetActive(!fullOn);
            Canvas.ForceUpdateCanvases();
            minimapCanvas.transform.SetAsLastSibling();

            CanvasGroup cg = minimapCanvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    private void LogStateChange(string source)
    {
        string currentState = "IQM "
            + (inventoryOn ? "1" : "0")
            + (questOn ? "1" : "0")
            + (fullOn ? "1" : "0");

        string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log("[" + timeStamp + "] " + source + ": " + previousState + " -> " + currentState);
        previousState = currentState;
    }
}
