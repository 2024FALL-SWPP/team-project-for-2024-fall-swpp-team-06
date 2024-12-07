using System.Collections;
using UnityEngine;

public class CombinedCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // 타겟 Transform
    public Vector3 offset = new Vector3(0, 1.5f, -3f); // 카메라 오프셋
    public float rotationSpeed = 5f; // 회전 속도
    public float followSpeed = 10f; // 따라가는 속도
    public float verticalRotationLimit = 45f; // Y축 회전 제한 (상하 제한)

    [Header("Game Over Settings")]
    public GameManager gameManager; // GameManager와 연동
    public Transform headRigTransform; // 게임 오버 애니메이션용 Transform
    public Quaternion headRigRotation = Quaternion.Euler(-94, 90, 0); // 게임 오버 시 카메라 회전
    public Transform headTransform; // 게임 오버 후 카메라 위치
    public Vector3 headPosition = new Vector3(0, -0.17f, -0.03f); // 게임 오버 후 카메라 위치 오프셋

    private bool isGameOver = false;
    private float currentVerticalRotation = 0f; // 현재 Y축 회전

    private void Start()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver += HandleGameOver; // 게임 오버 이벤트 구독
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
    }

    private void LateUpdate()
    {
        if (isGameOver)
        {
            return;
        }

        if (target != null)
        {
            FollowTarget();
            RotateCamera();
        }
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // 현재 Y축 회전값 갱신
        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation - mouseY, -verticalRotationLimit, verticalRotationLimit);

        // 카메라 회전 적용
        Quaternion targetRotation = Quaternion.Euler(currentVerticalRotation, transform.eulerAngles.y + mouseX, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        // 1. 게임 오버 애니메이션 처리
        transform.SetParent(headRigTransform, true);
        transform.localRotation = headRigRotation;

        yield return new WaitForSeconds(gameManager.gameOverAnimationDuration);

        // 2. 게임 오버 후 최종 위치로 이동
        transform.SetParent(headTransform, true);
        transform.localPosition = headPosition;
        transform.localRotation = Quaternion.Euler(90 + headRigRotation.eulerAngles.x, 0, 0);
    }
}
