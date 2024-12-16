using UnityEngine;
using System;
using System.Collections;
using DevionGames.UIWidgets;

public class UIManager : MonoBehaviour
{
    public UIWidget inventoryWidget;
    public GameObject questPanel;
    public GameObject minimapCamera;
    public GameObject fullmapCamera;
    public GameObject minimapCanvas;

    private bool inventoryOn = false;
    private bool questOn = false; // 퀘스트 on/off를 단순히 이 bool로 관리
    private bool fullOn = false; // false=Partial(M=0), true=Full(M=1)

    private string previousState = "IQM 000";

    void Awake()
    {
        // I키 에셋 처리 해제
        if (inventoryWidget != null)
        {
            WidgetInputHandler.UnregisterInput(KeyCode.I, inventoryWidget);
        }
    }

    void Start()
    {
        // 초기: partial 모드
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
            // 인벤토리 닫히는 것 기다리는 로직을 없애고, I->Q든 뭐든 단순히 On/Off
            ToggleQuest();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            // 인벤토리 닫히는 것 기다리는 로직 유지 필요하다면 다시 추가할 수 있지만,
            // 우선 Q 문제 해결 위해 단순화한 코드 제안
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
            // 인벤토리 켤 때 다른 UI 끄기
            if (questOn)
            {
                questPanel.SetActive(false);
                questOn = false;
            }

            if (fullOn)
            {
                // full->partial
                fullmapCamera.SetActive(false);
                minimapCamera.SetActive(true);
                fullOn = false;
            }

            if (inventoryWidget != null) inventoryWidget.Show();
            inventoryOn = true;
        }
        else
        {
            // 인벤토리 끄기
            if (inventoryWidget != null) inventoryWidget.Close();
            inventoryOn = false;
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleInventory");
    }

    private void ToggleQuest()
    {
        // Q 토글 단순화
        if (!questOn)
        {
            // Q 켜기
            // 다른 UI 끄기
            if (inventoryOn)
            {
                inventoryWidget.Close();
                inventoryOn = false;
            }

            if (fullOn)
            {
                // full->partial
                fullmapCamera.SetActive(false);
                minimapCamera.SetActive(true);
                fullOn = false;
            }

            questPanel.SetActive(true);
            questOn = true;
        }
        else
        {
            // Q 끄기
            questPanel.SetActive(false);
            questOn = false;
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleQuest");
    }

    private void ToggleMinimapMode()
    {
        // M 토글
        // 다른 UI 끄기
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

        // partial<->full 토글
        if (!fullOn)
        {
            // partial->full
            minimapCamera.SetActive(false);
            fullmapCamera.SetActive(true);
            fullOn = true;
        }
        else
        {
            // full->partial
            fullmapCamera.SetActive(false);
            minimapCamera.SetActive(true);
            fullOn = false;
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
        // I=inventoryOn, Q=questOn, M=fullOn
        string currentState = "IQM "
            + (inventoryOn ? "1" : "0")
            + (questOn ? "1" : "0")
            + (fullOn ? "1" : "0");

        string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log("[" + timeStamp + "] " + source + ": " + previousState + " -> " + currentState);
        previousState = currentState;
    }
}
