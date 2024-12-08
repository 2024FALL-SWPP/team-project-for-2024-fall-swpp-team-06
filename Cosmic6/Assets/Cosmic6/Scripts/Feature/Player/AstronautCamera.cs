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
    public Vector3 initialCameraEulerAngles = new Vector3(15f, 0f, 0f);

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
    public float zoomSpeed = 5f;
    public float minZoomDistance = 1.5f;
    public float maxZoomDistance = 6.0f;

    [Header("Crosshair Settings")]
    public Sprite crosshairSprite;

    // 인벤토리/퀘스트 UI 상태
    private bool inventoryActive = false;
    private bool questActive = false;

    private float currentVerticalRotation = 0f;
    private float currentZoom = 0f;
    private Vector3 originalOffset;

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

        originalOffset = initialCameraOffset;
        transform.rotation = Quaternion.Euler(initialCameraEulerAngles);

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

        // 게임 시작 시 인벤토리/퀘스트 상태에 따라 Cursor/Crosshair 상태 결정
        // inventoryActive, questActive가 초기값 false라면 Cursor는 Off, Crosshair On
        // 만약 시작부터 인벤토리 UI를 켜고 싶다면 inventoryActive = true로 세팅
        UpdateUIState();
    }

    private void LateUpdate()
    {
        if (gameManager != null && gameManager.IsGameOver) return;

        HandleZoom();
        if (target != null)
        {
            FollowTarget();
            RotateCamera();
        }

        HandleCrosshairUIInteraction();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
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
            return; // UI 활성 시 회전 불가
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
        // 인벤토리나 퀘스트 UI 상태만으로 Cursor/Crosshair 결정
        if (inventoryActive || questActive)
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

    private void ApplyInventoryState(bool active)
    {
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
            // crosshair를 null로 적용하면 gameobject를 비활성화
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
}
