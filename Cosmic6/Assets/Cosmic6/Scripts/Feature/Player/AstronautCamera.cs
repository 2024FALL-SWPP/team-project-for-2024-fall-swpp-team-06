using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AstronautCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    public Vector3 initialCameraOffset = new Vector3(0f, 3f, -4f);
    public float rotationSpeed = 1f;
    public float turnSmoothing = 0.1f;
    public float followSpeed = 10f;
    public float verticalRotationLimit = 45f;

    [Header("Rotation Limits")]
    public Vector2 yawLimit = new Vector2(-45f, 45f);
    public Vector2 pitchLimit = new Vector2(-45f, 15f);

    [Header("Game Over Settings")]
    public GameManager gameManager;
    public Transform headRigTransform;
    public Quaternion headRigRotation = Quaternion.Euler(-94, 90, 0);
    public Transform headTransform;
    public Vector3 headPosition = new Vector3(0, -0.17f, -0.03f);

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoomDistance = 1.5f;
    public float maxZoomDistance = 6.0f;

    [Header("Crosshair Settings")]
    public Sprite crosshairSprite;

    [Header("UI States")]
    public GameObject inventoryGameObject;
    public GameObject questGameObject;

    // Auto recentre parameters
    public float idleTimeBeforeRecentering = 0f; // 입력 없을 때 몇 초 후 재중심 시작
    public float recenterSpeed = 1f;          // 재중심 속도(0으로 돌아가는 속도)

    private bool inventoryActive = false;
    private bool questActive = false;

    private float currentZoom = 0f;
    private Vector3 originalOffset;

    private Canvas crosshairCanvas;
    private Image crosshairImage;

    private EventSystem eventSystem;
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerData;

    // ThirdPersonCamera 유사 마우스/회전 변수
    private float mouseX;
    private float mouseY;
    private float smoothX;
    private float smoothY;
    private float smoothXVelocity;
    private float smoothYVelocity;

    private float idleTimer = 0f;
    private bool hasInputThisFrame = false;

    void Start()
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
            if (player != null) target = player.transform;
        }

        if (target == null)
            Debug.LogError("Target is null! Make sure the Player has the 'Player' tag.");

        originalOffset = initialCameraOffset;

        // 플레이어 각도로 초기 세팅
        if (target != null)
        {
            mouseY = target.eulerAngles.x;
            mouseX = target.eulerAngles.y;
        }

        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null) Debug.LogWarning("EventSystem not found!");
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        if (graphicRaycaster == null) Debug.LogWarning("GraphicRaycaster not found!");

        pointerData = new PointerEventData(EventSystem.current);

        UpdateUIState();
    }

    void LateUpdate()
    {
        if (gameManager != null && gameManager.IsGameOver) return;

        bool newInventoryActive = (inventoryGameObject != null && inventoryGameObject.activeSelf);
        bool newQuestActive = (questGameObject != null && questGameObject.activeSelf);

        if (newInventoryActive != inventoryActive || newQuestActive != questActive)
        {
            inventoryActive = newInventoryActive;
            questActive = newQuestActive;
            UpdateUIState();
        }

        UpdateInput();
        HandleZoom();
        if (target != null)
        {
            UpdateTransform();
        }

        HandleCrosshairUIInteraction();
    }

    private void UpdateInput()
    {
        hasInputThisFrame = false;

        // UI 활성 시 회전 불가
        if (inventoryActive || questActive) return;

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(x) > 0.01f || Mathf.Abs(y) > 0.01f)
        {
            // 마우스 입력 있을 때 yaw, pitch 갱신
            mouseX += x * rotationSpeed;
            mouseY -= y * rotationSpeed;
            mouseX = ClampAngle(mouseX, yawLimit.x, yawLimit.y);
            mouseY = ClampAngle(mouseY, pitchLimit.x, pitchLimit.y);

            hasInputThisFrame = true;
            idleTimer = 0f; // 입력 있으면 idleTimer 리셋
        }
        else
        {
            // 입력 없음
            idleTimer += Time.deltaTime;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoomDistance - originalOffset.magnitude, maxZoomDistance - originalOffset.magnitude);
    }

    private void UpdateTransform()
    {
        // UI 활성 시 회전 반영 안 함
        if (!(inventoryActive || questActive))
        {
            // 입력 없으면 일정시간 후 yaw=0으로 복귀
            if (!hasInputThisFrame && idleTimer > idleTimeBeforeRecentering)
            {
                // mouseX를 0으로 서서히 이동
                mouseX = Mathf.MoveTowards(mouseX, 0f, recenterSpeed * Time.deltaTime * Mathf.Abs(mouseX));
            }

            smoothX = Mathf.SmoothDamp(smoothX, mouseX, ref smoothXVelocity, turnSmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, mouseY, ref smoothYVelocity, turnSmoothing);

            Quaternion targetRotation = Quaternion.Euler(smoothY, smoothX, 0);
            transform.rotation = targetRotation;
        }

        Vector3 zoomedOffset = originalOffset.normalized * (originalOffset.magnitude + currentZoom);
        Vector3 desiredPosition = target.position + zoomedOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
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

    private void UpdateUIState()
    {
        bool uiActive = inventoryActive || questActive;
        if (uiActive)
        {
            // UI 활성: Cursor On, Crosshair Off, 회전 불가
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ApplyCrosshair(null);
        }
        else
        {
            // UI 비활성: Cursor Off, Crosshair On, 회전 가능
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
        // UI 활성 시 crosshair 상호작용 X
        if (inventoryActive || questActive) return;

        // Cursor visible이면 crosshair 상호작용 X
        if (Cursor.visible) return;

        // Crosshair 활성 & Cursor 비활성 상태에서만 작동
        if (crosshairImage != null && crosshairImage.gameObject.activeSelf && !Cursor.visible)
        {
            if (graphicRaycaster == null || eventSystem == null) return;

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

    private float ClampAngle(float angle, float min, float max)
    {
        do
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
        } while (angle < -360f || angle > 360f);

        return Mathf.Clamp(angle, min, max);
    }
}
