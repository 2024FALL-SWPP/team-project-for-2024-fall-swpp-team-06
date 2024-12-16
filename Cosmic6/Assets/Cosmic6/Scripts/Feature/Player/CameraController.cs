using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public GameManager gameManager;

    public float horizontalSensitivity = 150f;
    public float verticalSensitivity = 100f;
    public float xRotationLimit = 89f;

    public GameObject player;
    private Transform playerTransform;

    public Transform headRigTransform;
    private Quaternion headRigRotation = Quaternion.Euler(-94, 90, 0);

    public Transform headTransform;
    private Vector3 headPosition = new(0, -0.17f, -0.03f);

    private float xRotation = 0f;

    // UI 관련
    public GameObject inventoryGameObject;
    public GameObject questGameObject;
    public GameObject minimapCamera;

    private bool inventoryActive = false;
    private bool questActive = false;
    private bool minimapActive = false;
    // minimapActive는 minimapCamera가 비활성화(!activeSelf)일 때 true로 설정

    // isUIActive에 minimapActive 포함
    private bool isUIActive => inventoryActive || questActive || minimapActive;

    // Crosshair 관련
    public Sprite crosshairSprite;
    private Canvas crosshairCanvas;
    private Image crosshairImage;

    // UI 상호작용
    private EventSystem eventSystem;
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerData;

    void Start()
    {
        gameManager.OnGameOver += GameOver;
        playerTransform = player.transform;

        // EventSystem, GraphicRaycaster 찾기
        eventSystem = FindObjectOfType<EventSystem>();
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        pointerData = new PointerEventData(EventSystem.current);

        UpdateUIState();
    }

    void Update()
    {
        if (gameManager.IsGameOver) return;

        // UI 상태 갱신
        bool newInventoryActive = (inventoryGameObject != null && inventoryGameObject.activeSelf);
        bool newQuestActive = (questGameObject != null && questGameObject.activeSelf);
        bool newMinimapActive = (minimapCamera != null && !minimapCamera.activeSelf);
        // minimapCamera가 비활성화일 때 minimapActive = true

        if (newInventoryActive != inventoryActive || newQuestActive != questActive || newMinimapActive != minimapActive)
        {
            inventoryActive = newInventoryActive;
            questActive = newQuestActive;
            minimapActive = newMinimapActive;
            UpdateUIState();
        }

        if (!isUIActive)
        {
            // UI 비활성 상태일 때만 카메라 회전 처리
            float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);

            playerTransform.Rotate(Vector3.up * mouseX);
            transform.localRotation = Quaternion.Euler(90 + xRotation, 0f, 0f);
        }

        HandleCrosshairUIInteraction();
    }

    private void UpdateUIState()
    {
        bool uiActive = isUIActive;
        if (uiActive)
        {
            // UI 활성: Cursor On, Crosshair Off
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ApplyCrosshair(null);
        }
        else
        {
            // UI 비활성: Cursor Off, Crosshair On
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ApplyCrosshair(crosshairSprite);
        }
    }

    private void ApplyCrosshair(Sprite crosshair)
    {
        if (crosshairImage == null)
        {
            CreateCrosshairUI();
        }

        if (crosshair != null)
        {
            crosshairImage.sprite = crosshair;
            crosshairImage.SetNativeSize();
            crosshairImage.gameObject.SetActive(true);
        }
        else
        {
            if (crosshairImage != null) crosshairImage.gameObject.SetActive(false);
        }
    }

    private void CreateCrosshairUI()
    {
        GameObject canvasGameObject = new GameObject("Crosshair Canvas");
        crosshairCanvas = canvasGameObject.AddComponent<Canvas>();
        crosshairCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        crosshairCanvas.pixelPerfect = true;
        crosshairCanvas.overrideSorting = true;
        crosshairCanvas.sortingOrder = 100;

        GameObject crosshairGameObject = new GameObject("Crosshair");
        crosshairImage = crosshairGameObject.AddComponent<Image>();
        crosshairGameObject.transform.SetParent(canvasGameObject.transform, false);
        crosshairGameObject.SetActive(false);

        DontDestroyOnLoad(canvasGameObject);
    }

    private void HandleCrosshairUIInteraction()
    {
        // UI 활성 상태이면 커서로 자유롭게 UI 상호작용 (Unity 기본 UI 상호작용)
        if (isUIActive || Cursor.visible)
        {
            eventSystem.SetSelectedGameObject(null);
            return;
        }

        // UI 비활성 상태에서 crosshair 상호작용
        if (crosshairImage != null && crosshairImage.gameObject.activeSelf && !Cursor.visible)
        {
            if (graphicRaycaster == null || eventSystem == null) return;

            // 화면 중앙에 Raycast
            pointerData.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerData, results);

            if (results.Count > 0)
            {
                GameObject hovered = results[0].gameObject;
                eventSystem.SetSelectedGameObject(hovered);

                if (Input.GetMouseButtonDown(0))
                {
                    ExecuteEvents.Execute(hovered, pointerData, ExecuteEvents.pointerClickHandler);
                }
            }
            else
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }

    void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        transform.SetParent(headRigTransform, true);
        transform.localRotation = headRigRotation;

        yield return new WaitForSeconds(gameManager.gameOverAnimationDuration);

        transform.SetParent(headTransform, true);
        transform.localPosition = headPosition;
        transform.localRotation = Quaternion.Euler(90 + xRotation, 0, 0f);
    }
}
