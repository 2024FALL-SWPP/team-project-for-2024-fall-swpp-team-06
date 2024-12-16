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
    private bool questOn = false;
    private bool fullOn = false; // false = Partial Mode(M=0), true = Full Mode(M=1)

    private string previousState = "IQM 000";

    void Awake()
    {
        // I키 해제해서 UIManager에서 I키 관리
        if (inventoryWidget != null)
        {
            WidgetInputHandler.UnregisterInput(KeyCode.I, inventoryWidget);
        }
    }

    void Start()
    {
        // 초기상태: Partial Mode ON (fullOn=false)
        if (minimapCamera != null) minimapCamera.SetActive(true);
        if (fullmapCamera != null) fullmapCamera.SetActive(false);
        if (questPanel != null) questPanel.SetActive(false);

        // 초기에는 인벤토리 off, 퀘스트 off, fullOn=false(Partial)
        inventoryOn = false;
        questOn = false;
        fullOn = false; // partial mode

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
            // I->Q 시나리오에서만 인벤토리 닫히는거 기다림
            if (inventoryOn)
            {
                // 인벤토리 닫고 기다린 뒤 퀘스트 켜기
                inventoryWidget.Close();
                inventoryOn = false;
                LogStateChange("PreparingForQuest");
                StartCoroutine(WaitForInventoryClose(() => ToggleQuestActual()));
            }
            else
            {
                // 인벤토리가 꺼져있다면 즉시 토글
                ToggleQuestActual();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            // I->M 시나리오에서만 인벤토리 닫히는거 기다림
            if (inventoryOn)
            {
                inventoryWidget.Close();
                inventoryOn = false;
                LogStateChange("PreparingForMinimap");
                StartCoroutine(WaitForInventoryClose(() => ToggleMinimapModeActual()));
            }
            else
            {
                // 인벤토리가 꺼져있다면 즉시 토글
                ToggleMinimapModeActual();
            }
        }
    }

    public void CloseInventoryFromButton()
    {
        // X 버튼으로 인벤토리 닫기
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
            // 인벤토리 켜기
            // 인벤토리를 켜기 전에 퀘스트, 풀맵(Full모드)이 켜져있다면 끄고 partial로 돌아옴
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
                fullOn = false; // partial mode
            }

            if (inventoryWidget != null) inventoryWidget.Show();
            inventoryOn = true;
        }
        else
        {
            // 인벤토리 끄기
            if (inventoryWidget != null) inventoryWidget.Close();
            inventoryOn = false;
            // partial 유지
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleInventory");
    }

    // 인벤토리 닫힌 뒤 퀘스트 토글
    private void ToggleQuestActual()
    {
        bool questActive = questOn;

        if (!questActive)
        {
            // 퀘스트 켜기
            if (fullOn)
            {
                // full->partial
                fullmapCamera.SetActive(false);
                minimapCamera.SetActive(true);
                fullOn = false;
            }

            if (questPanel != null) questPanel.SetActive(true);
            questOn = true;
        }
        else
        {
            // 퀘스트 끄기
            if (questPanel != null) questPanel.SetActive(false);
            questOn = false;
            // partial 유지
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleQuest");
    }

    // 인벤토리 닫힌 뒤 미니맵모드 토글
    private void ToggleMinimapModeActual()
    {
        // M키: partial <-> full 모드 전환
        if (questOn)
        {
            questPanel.SetActive(false);
            questOn = false;
        }

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
            if (!fullOn)
            {
                // partial
                minimapCanvas.SetActive(true);
            }
            else
            {
                // full
                minimapCanvas.SetActive(false);
            }
        }
    }

    private void LogStateChange(string source)
    {
        // I=inventoryOn, Q=questOn, M=fullOn
        // fullOn=false -> M=0(partial)
        // fullOn=true -> M=1(full)
        string currentState = "IQM "
            + (inventoryOn ? "1" : "0")
            + (questOn ? "1" : "0")
            + (fullOn ? "1" : "0");

        string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log("[" + timeStamp + "] " + source + ": " + previousState + " -> " + currentState);
        previousState = currentState;
    }

    private IEnumerator WaitForInventoryClose(Action onClosed)
    {
        // 인벤토리가 완전히 닫힐 때까지 대기 (IsVisible false일 때까지)
        while (inventoryWidget != null && inventoryWidget.IsVisible)
        {
            yield return null;
        }
        onClosed?.Invoke();
    }
}