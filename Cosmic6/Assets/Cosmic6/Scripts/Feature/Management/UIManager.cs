using UnityEngine;
using System;
using DevionGames.UIWidgets;

public class UIManager : MonoBehaviour
{
    public UIWidget inventoryWidget; // 인벤토리 위젯 (I키는 에셋에서 처리)
    public QuestSystem questSystem;  // 퀘스트 시스템
    public GameObject minimapCamera; // partial 모드 카메라
    public GameObject fullmapCamera; // full 모드 카메라
    public GameObject minimapCanvas; // partial 일 때만 On

    private string previousState = "IQM 000";

    void Start()
    {
        // 시작 시 모두 꺼짐
        if (questSystem != null && questSystem.questPanel != null)
            questSystem.questPanel.SetActive(false);
        if (minimapCamera != null) minimapCamera.SetActive(false);
        if (fullmapCamera != null) fullmapCamera.SetActive(false);

        UpdateMinimapCanvasState();
        LogStateChange();
    }

    void Update()
    {
        // Q키 눌러 퀘스트 패널 토글
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuest();
        }

        // M키 눌러 미니맵 토글
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMinimapCameras();
        }

        // I키는 에셋 코드에서 처리하므로 여기서는 처리하지 않는다.
    }

    private void ToggleQuest()
    {
        bool questActive = (questSystem != null && questSystem.questPanel != null && questSystem.questPanel.activeSelf);

        // Q를 눌렀을 때 퀘스트를 토글
        // 퀘스트 활성화하기 전에 인벤토리 강제 비활성화
        if (inventoryWidget != null && inventoryWidget.IsVisible)
            inventoryWidget.Close(); // 인벤토리 닫기

        // 미니맵도 꺼준다
        if (minimapCamera != null) minimapCamera.SetActive(false);
        if (fullmapCamera != null) fullmapCamera.SetActive(false);

        // 퀘스트 토글
        if (questSystem != null && questSystem.questPanel != null)
        {
            questSystem.SetQuestPanelActive(!questActive);
        }

        UpdateMinimapCanvasState();
        LogStateChange();
    }

    private void ToggleMinimapCameras()
    {
        bool partialActive = (minimapCamera != null && minimapCamera.activeSelf);
        bool fullActive = (fullmapCamera != null && fullmapCamera.activeSelf);

        // M 누르면 미니맵 모드 토글
        // 이때 인벤토리 강제 비활성화
        if (inventoryWidget != null && inventoryWidget.IsVisible)
            inventoryWidget.Close();

        // 퀘스트도 끈다
        if (questSystem != null && questSystem.questPanel != null && questSystem.questPanel.activeSelf)
            questSystem.SetQuestPanelActive(false);

        // 미니맵 토글 로직
        if (!partialActive && !fullActive)
        {
            // 둘 다 꺼져있으면 partial 켬
            if (minimapCamera != null) minimapCamera.SetActive(true);
            if (fullmapCamera != null) fullmapCamera.SetActive(false);
        }
        else if (partialActive)
        {
            // partial -> full
            if (minimapCamera != null) minimapCamera.SetActive(false);
            if (fullmapCamera != null) fullmapCamera.SetActive(true);
        }
        else if (fullActive)
        {
            // full -> partial
            if (fullmapCamera != null) fullmapCamera.SetActive(false);
            if (minimapCamera != null) minimapCamera.SetActive(true);
        }

        UpdateMinimapCanvasState();
        LogStateChange();
    }

    private void UpdateMinimapCanvasState()
    {
        bool partialActive = (minimapCamera != null && minimapCamera.activeSelf);
        bool fullActive = (fullmapCamera != null && fullmapCamera.activeSelf);

        if (minimapCanvas != null)
        {
            if (partialActive && !fullActive)
            {
                minimapCanvas.SetActive(true);
            }
            else
            {
                minimapCanvas.SetActive(false);
            }
        }
    }

    private void LogStateChange()
    {
        bool invVisible = (inventoryWidget != null && inventoryWidget.IsVisible);
        bool questVisible = (questSystem != null && questSystem.questPanel != null && questSystem.questPanel.activeSelf);
        bool fullActive = (fullmapCamera != null && fullmapCamera.activeSelf);

        bool fullMapVisible = fullActive;

        string currentState = "IQM "
            + (invVisible ? "1" : "0")
            + (questVisible ? "1" : "0")
            + (fullMapVisible ? "1" : "0");

        if (currentState != previousState)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.Log("[" + timeStamp + "] " + previousState + " -> " + currentState);
            previousState = currentState;
        }
    }
}
