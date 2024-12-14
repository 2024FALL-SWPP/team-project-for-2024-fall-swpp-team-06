using UnityEngine;
using DevionGames.UIWidgets; // UIWidget 관련 클래스 참조를 위해

public class UIManager : MonoBehaviour
{
    public UIWidget inventoryWidget;    // 인스펙터에서 인벤토리 UIWidget 할당
    public QuestSystem questSystem;     // 인스펙터에서 QuestSystem 할당 (questPanel 접근용)
    public UIWidget minimapWidget;      // 인스펙터에서 Minimap UIWidget 할당
    public MinimapController minimapController; // Minimap 상태(Partial/Full) 제어용

    void Update()
    {
        // Q와 M 키를 여기서 처리
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuest();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMinimap();
        }

        // I 키는 에셋 코드 그대로 두었으므로 여기서 처리 안 함.
        // 대신 매 프레임 UI 상태를 체크해서 한 UI만 열리도록 유지
        EnforceSingleUIVisible();
    }

    private void ToggleQuest()
    {
        // 다른 UI 끄기
        if (inventoryWidget != null && inventoryWidget.IsVisible)
            inventoryWidget.Close();
        if (minimapWidget != null && minimapWidget.IsVisible)
            minimapWidget.Close();

        // 퀘스트 패널 토글
        if (questSystem != null && questSystem.questPanel != null)
        {
            bool currentlyActive = questSystem.questPanel.activeSelf;
            questSystem.SetQuestPanelActive(!currentlyActive);
        }
    }

    private void ToggleMinimap()
    {
        // 다른 UI 끄기
        if (inventoryWidget != null && inventoryWidget.IsVisible)
            inventoryWidget.Close();
        if (questSystem != null && questSystem.questPanel.activeSelf)
            questSystem.SetQuestPanelActive(false);

        // 미니맵 UI 토글 로직
        if (minimapWidget != null && minimapController != null)
        {
            if (!minimapWidget.IsVisible)
            {
                // 미니맵이 꺼져있다면 기본상태(Partial)로 켜기
                minimapWidget.Show();
                minimapController.CloseFullMap();
                // ShowFullMap() -> Full 화면, CloseFullMap() -> Partial 화면
                // 여기서 CloseFullMap() 호출로 Partial 상태로 진입
            }
            else
            {
                // 미니맵이 이미 켜져있다면 Partial <-> Full 상태 토글
                if (minimapController.IsFullMap())
                {
                    // Full 상태에서 Partial로
                    minimapController.CloseFullMap();
                }
                else
                {
                    // Partial 상태에서 Full 상태로
                    minimapController.ShowFullMap();
                }
            }
        }
    }

    private void EnforceSingleUIVisible()
    {
        bool invVisible = (inventoryWidget != null && inventoryWidget.IsVisible);
        bool questVisible = (questSystem != null && questSystem.questPanel.activeSelf);
        bool mapVisible = (minimapWidget != null && minimapWidget.IsVisible);

        // 인벤토리가 켜져 있으면 퀘스트, 미니맵 끄기
        if (invVisible)
        {
            if (questVisible)
                questSystem.SetQuestPanelActive(false);
            if (mapVisible)
                minimapWidget.Close();
        }

        // 퀘스트가 켜져 있으면 인벤토리, 미니맵 끄기
        if (questVisible)
        {
            if (invVisible)
                inventoryWidget.Close();
            if (mapVisible)
                minimapWidget.Close();
        }

        // 미니맵이 켜져 있으면 인벤토리, 퀘스트 끄기
        if (mapVisible)
        {
            if (invVisible)
                inventoryWidget.Close();
            if (questVisible)
                questSystem.SetQuestPanelActive(false);
        }
    }
}
