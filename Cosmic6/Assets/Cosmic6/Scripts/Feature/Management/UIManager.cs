using UnityEngine;
using System; // DateTime 사용
using DevionGames.UIWidgets; // 인벤토리 UIWidget을 사용하기 위해 필요

public class UIManager : MonoBehaviour
{
    public UIWidget inventoryWidget; // 인벤토리 UIWidget (I키로 토글)
    public QuestSystem questSystem;  // 퀘스트 시스템 (questPanel 제어용)
    public GameObject minimapCamera; // 미니맵 카메라 GameObject (Partial)
    public GameObject fullmapCamera; // 풀맵 카메라 GameObject (Full)

    // 이전 상태 기억용
    private string previousState = "IQM 000"; // 시작 시 아무것도 안 켜져있다고 가정

    void Start()
    {
        if (minimapCamera != null)
            minimapCamera.SetActive(true);
        if (fullmapCamera != null)
            fullmapCamera.SetActive(false);
    }

    void Update()
    {
        // Q키 처리: 퀘스트 토글
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuest();
        }

        // M키 처리: 미니맵 partial <-> full 토글
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMinimapCameras();
        }

        // I키는 에셋 코드에서 처리 (수정 없음)
        // 매 프레임 한 UI만 보이도록 상태 유지
        EnforceSingleUIVisible();
    }

    private void ToggleQuest()
    {
        // 다른 UI 끄기
        if (inventoryWidget != null && inventoryWidget.IsVisible)
            inventoryWidget.Close();

        if (minimapCamera != null && minimapCamera.activeSelf)
            minimapCamera.SetActive(false);
        if (fullmapCamera != null && fullmapCamera.activeSelf)
            fullmapCamera.SetActive(false);

        // 퀘스트 패널 토글
        if (questSystem != null && questSystem.questPanel != null)
        {
            bool currentlyActive = questSystem.questPanel.activeSelf;
            questSystem.SetQuestPanelActive(!currentlyActive);
        }
    }

    private void ToggleMinimapCameras()
    {
        // 다른 UI 끄기 (인벤토리, 퀘스트)
        if (inventoryWidget != null && inventoryWidget.IsVisible)
            inventoryWidget.Close();
        if (questSystem != null && questSystem.questPanel.activeSelf)
            questSystem.SetQuestPanelActive(false);

        // 현재 미니맵 상태 파악
        bool partialActive = (minimapCamera != null && minimapCamera.activeSelf);
        bool fullActive = (fullmapCamera != null && fullmapCamera.activeSelf);

        // partial <-> full 상태 토글 로직
        if (!partialActive && !fullActive)
        {
            // 둘 다 꺼져있으면 partial(미니맵카메라) 켬
            if (minimapCamera != null)
                minimapCamera.SetActive(true);
            if (fullmapCamera != null)
                fullmapCamera.SetActive(false);
        }
        else if (partialActive)
        {
            // partial 켜져있으면 full 켜고 partial 끔
            if (minimapCamera != null)
                minimapCamera.SetActive(false);
            if (fullmapCamera != null)
                fullmapCamera.SetActive(true);
        }
        else if (fullActive)
        {
            // full 켜져있으면 partial 켜고 full 끔
            if (fullmapCamera != null)
                fullmapCamera.SetActive(false);
            if (minimapCamera != null)
                minimapCamera.SetActive(true);
        }
    }

    private void EnforceSingleUIVisible()
    {
        bool invVisible = (inventoryWidget != null && inventoryWidget.IsVisible);
        bool questVisible = (questSystem != null && questSystem.questPanel.activeSelf);
        bool partialActive = (minimapCamera != null && minimapCamera.activeSelf);
        bool fullActive = (fullmapCamera != null && fullmapCamera.activeSelf);
        
        // 여기서 M은 fullmapCamera가 켜져 있을 때만 1, 아니면 0
        bool fullMapVisible = fullActive; 

        // 인벤토리가 켜져 있으면 다른 UI 끄기
        if (invVisible)
        {
            if (questVisible)
                questSystem.SetQuestPanelActive(false);
            if (partialActive || fullActive)
            {
                if (minimapCamera != null) minimapCamera.SetActive(false);
                if (fullmapCamera != null) fullmapCamera.SetActive(false);
            }
        }

        // 퀘스트가 켜져 있으면 다른 UI 끄기
        if (questVisible)
        {
            if (invVisible)
                inventoryWidget.Close();
            if (partialActive || fullActive)
            {
                if (minimapCamera != null) minimapCamera.SetActive(false);
                if (fullmapCamera != null) fullmapCamera.SetActive(false);
            }
        }

        // 미니맵이 켜져 있으면 다른 UI 끄기
        if (partialActive || fullActive)
        {
            if (invVisible)
                inventoryWidget.Close();
            if (questVisible)
                questSystem.SetQuestPanelActive(false);
        }

        // 현재 상태 문자열 생성
        // I: invVisible, Q: questVisible, M: fullMapVisible(FullMap 상태만)
        string currentState = "IQM " 
            + (invVisible ? "1" : "0") 
            + (questVisible ? "1" : "0") 
            + (fullMapVisible ? "1" : "0");

        // 상태가 바뀌었으면 로그 출력(타임스탬프 포함)
        if (currentState != previousState)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.Log("[" + timeStamp + "] " + previousState + " -> " + currentState);
            previousState = currentState;
        }
    }
}
