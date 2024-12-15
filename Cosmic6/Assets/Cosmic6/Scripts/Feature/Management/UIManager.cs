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
        // partial mode: minimapCamera ON, fullmapCamera OFF, minimapCanvas ON
        if (minimapCamera != null) minimapCamera.SetActive(true);
        if (fullmapCamera != null) fullmapCamera.SetActive(false);
        if (questPanel != null) questPanel.SetActive(false);
        UpdateMinimapCanvasState();

        // 초기에는 인벤토리 off, 퀘스트 off, fullOn=false(Partial)
        inventoryOn = false;
        questOn = false;
        fullOn = false; // partial mode

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
        // X 버튼으로 인벤토리 닫기
        if (inventoryOn && inventoryWidget != null)
        {
            inventoryWidget.Close();
            inventoryOn = false;
            // 인벤토리 off시 partial mode 유지
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

            // 만약 fullOn=true였다면 partial로 돌려야 함
            if (fullOn)
            {
                // full->partial
                fullmapCamera.SetActive(false);
                minimapCamera.SetActive(true);
                fullOn = false; // partial mode
            }

            // 이제 partial mode 상태에서 인벤토리 켜기
            if (inventoryWidget != null) inventoryWidget.Show();
            inventoryOn = true;
        }
        else
        {
            // 인벤토리 끄기
            if (inventoryWidget != null) inventoryWidget.Close();
            inventoryOn = false;
            // 끌 때도 partial 유지
        }

        UpdateMinimapCanvasState();
        LogStateChange("ToggleInventory");
    }

    private void ToggleQuest()
    {
        bool questActive = questOn;

        if (!questActive)
        {
            // 퀘스트 켜기
            // 인벤토리나 full모드 켜져있다면 끄고 partial로 전환
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

            // 이제 partial 상태에서 퀘스트 켜기
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

    private void ToggleMinimapMode()
    {
        // M키: partial <-> full 모드 전환
        // 인벤토리나 퀘스트 켜져있으면 끄고 전환
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

        // 이제 partial <-> full 토글
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
        // partial일 때 minimapCanvas ON
        // full일 때 canvas OFF
        // partial = !fullOn
        // full = fullOn

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
}
