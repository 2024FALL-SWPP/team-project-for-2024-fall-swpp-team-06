using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CombinedCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, -3f);
    public float rotationSpeed = 5f;
    public float followSpeed = 10f;
    public float verticalRotationLimit = 45f;

    [Header("Game Over Settings")]
    public GameManager gameManager;
    public Transform headRigTransform;
    public Quaternion headRigRotation = Quaternion.Euler(-94, 90, 0);
    public Transform headTransform;
    public Vector3 headPosition = new Vector3(0, -0.17f, -0.03f);

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoomDistance = 1f;
    public float maxZoomDistance = 10f;

    [Header("Crosshair Settings")]
    public Sprite crosshairSprite; // Inspector에서 할당할 Crosshair 스프라이트

    private bool inventoryActive = false;
    private bool questActive = false; // 추가
    private bool rotatedLastFrame = false;
    private float currentVerticalRotation = 0f;
    private float currentZoom = 0f;
    private Vector3 originalOffset;
    private CursorLockMode cursorModeWhenInventoryOff = CursorLockMode.Locked;
    private float visibilityDelta = 0.1f;
    private bool consumeTurn = false;

    private Canvas crosshairCanvas;
    private Image crosshairImage;

    private EventSystem eventSystem;
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerData;

    private void Start()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver += HandleGameOver;
        }
        else
        {
            Debug.LogError("GameManager is null!");
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target == null)
        {
            Debug.LogError("Target is null! Make sure the Player has the 'Player' tag.");
        }

        originalOffset = offset;
        ApplyInventoryState(false);

        // UI 상호작용용 EventSystem, GraphicRaycaster 찾기
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("EventSystem not found. UI Interaction won't work.");
        }
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        if (graphicRaycaster == null)
        {
            Debug.LogWarning("GraphicRaycaster not found. UI Interaction won't work.");
        }

        pointerData = new PointerEventData(EventSystem.current);
    }

    private void LateUpdate()
    {
        if (gameManager != null && gameManager.IsGameOver) return;

        UpdateInput();
        if (target != null)
        {
            HandleZoom();
            FollowTarget();
            RotateCamera();
        }

        // Crosshair가 활성 & Cursor 비활성일 때 화면 중앙 기반 UI 상호작용 처리
        HandleCrosshairUIInteraction();
    }

    private void UpdateInput()
    {
        consumeTurn = (inventoryActive || questActive);

        if (inventoryActive || questActive) return;

        float x = Input.GetAxis("Mouse X") * rotationSpeed;
        float y = Input.GetAxis("Mouse Y") * rotationSpeed;

        if (!consumeTurn && (Mathf.Abs(x) > visibilityDelta || Mathf.Abs(y) > visibilityDelta))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            rotatedLastFrame = true;
        }
        else if (rotatedLastFrame)
        {
            Cursor.lockState = cursorModeWhenInventoryOff;
            Cursor.visible = (cursorModeWhenInventoryOff == CursorLockMode.Locked ? false : true);
            rotatedLastFrame = false;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        // Zoom 방향 반전
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoomDistance - originalOffset.magnitude, maxZoomDistance - originalOffset.magnitude);
    }

    private void FollowTarget()
    {
        Vector3 zoomedOffset = originalOffset.normalized * (originalOffset.magnitude + currentZoom);
        Vector3 desiredPosition = target.position + zoomedOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    private void RotateCamera()
    {
        if (inventoryActive || questActive)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation - mouseY, -verticalRotationLimit, verticalRotationLimit);
        Quaternion targetRotation = Quaternion.Euler(currentVerticalRotation, transform.eulerAngles.y + mouseX, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleGameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        transform.SetParent(headRigTransform, true);
        transform.localRotation = headRigRotation;

        yield return new WaitForSeconds(gameManager.gameOverAnimationDuration);

        transform.SetParent(headTransform, true);
        transform.localPosition = headPosition;
        transform.localRotation = Quaternion.Euler(90 + headRigRotation.eulerAngles.x, 0, 0);
    }

    public void OnInventoryToggle(bool active)
    {
        inventoryActive = active;
        UpdateUIState();
    }

    public void OnQuestToggle(bool active)
    {
        questActive = active;
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        // 인벤토리나 퀘스트 중 하나라도 활성화면 커서 ON & 크로스헤어 OFF
        // 둘 다 비활성화면 커서 OFF & 크로스헤어 ON
        if (inventoryActive || questActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ApplyCrosshair(null);
        }
        else
        {
            Cursor.lockState = cursorModeWhenInventoryOff;
            Cursor.visible = (cursorModeWhenInventoryOff == CursorLockMode.Locked ? false : true);
            ApplyCrosshair(crosshairSprite);
        }
    }

    private void ApplyInventoryState(bool active)
    {
        // 초기 상태 설정용 메서드였으나 이제 UpdateUIState로 일원화 가능
        inventoryActive = active;
        UpdateUIState();
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
            crosshairImage.gameObject.SetActive(false);
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
        // Crosshair 활성 & Cursor 비활성 상태에서만 동작
        if (crosshairImage != null && crosshairImage.gameObject.activeSelf && !Cursor.visible)
        {
            if (graphicRaycaster == null || eventSystem == null) return;

            // 화면 중앙 지점 기준으로 UI Raycast
            pointerData.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerData, results);

            if (results.Count > 0)
            {
                // Hover 처리
                GameObject hovered = results[0].gameObject;
                eventSystem.SetSelectedGameObject(hovered);

                // 클릭 처리
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
}
