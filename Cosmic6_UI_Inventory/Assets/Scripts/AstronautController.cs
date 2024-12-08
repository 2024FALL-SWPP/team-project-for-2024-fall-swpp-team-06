using UnityEngine;

public class AstronautController : MonoBehaviour
{
    public float maxSpeed = 1f;              // 최대 속력
    public float speedIncreaseRate = 0.3f;   // 속도 증가율
    public float speedDecreaseRate = 2f;     // 속도 감소율
    public float rotationSpeed = 1f;         // 회전 속도
    public float rotationThreshold = 0.001f; // 회전 방향 결정용 임계값

    private Animator animator;
    private float currentSpeed = 0f;
    private Vector3 desiredDirection = Vector3.zero;
    private Vector3 currentDirection = Vector3.forward; // 현재 바라보는 방향 (초기값)

    private Transform mainCamera; // 메인 카메라 참조용

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator is missing from the AstronautController!");
        }

        // 메인 카메라 참조
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Camera not found!");
        }
    }

    private void Update()
    {
        HandleMovement();
        UpdateAnimatorParameters();
    }

    private void HandleMovement()
    {
        // 입력 받기
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // 카메라 기준 방향 벡터
        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;

        // 수평 정규화
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 방향키 입력 조합 처리
        Vector3 inputDirection = Vector3.zero;

        bool verticalConflict = up && down;
        bool horizontalConflict = left && right;

        if (!verticalConflict && !horizontalConflict)
        {
            if (up && !down && !left && !right)
            {
                inputDirection = cameraForward;
            }
            else if (down && !up && !left && !right)
            {
                inputDirection = -cameraForward;
            }
            else if (left && !right && !up && !down)
            {
                inputDirection = -cameraRight;
            }
            else if (right && !left && !up && !down)
            {
                inputDirection = cameraRight;
            }
            else if (up && left && !down && !right)
            {
                inputDirection = (cameraForward - cameraRight).normalized;
            }
            else if (up && right && !down && !left)
            {
                inputDirection = (cameraForward + cameraRight).normalized;
            }
            else if (down && left && !up && !right)
            {
                inputDirection = (-cameraForward - cameraRight).normalized;
            }
            else if (down && right && !up && !left)
            {
                inputDirection = (-cameraForward + cameraRight).normalized;
            }
            else
            {
                // 입력 없음
                inputDirection = Vector3.zero;
            }
        }
        else
        {
            // 상하/좌우 동시 입력 시 무시
            inputDirection = Vector3.zero;
        }

        // 회전 처리
        if (inputDirection != Vector3.zero)
        {
            desiredDirection = inputDirection;
        }

        if (desiredDirection != Vector3.zero && currentDirection != Vector3.zero)
        {
            float angle = Vector3.SignedAngle(currentDirection, desiredDirection, Vector3.up);
            // tie situation(180도)에서 반시계 방향(양수 angle) 선택
            if (Mathf.Abs(angle) == 180f)
            {
                angle = Mathf.Abs(angle); // 180으로 고정, 양수면 반시계
            }

            if (Mathf.Abs(angle) > rotationThreshold)
            {
                // angle 절대값만큼 회전 속도 적용
                float rotateAngle = Mathf.MoveTowardsAngle(0, angle, rotationSpeed * Time.deltaTime * Mathf.Abs(angle));
                Quaternion rot = Quaternion.AngleAxis(rotateAngle, Vector3.up);
                currentDirection = rot * currentDirection;
            }
        }

        // 속도 처리
        if (inputDirection != Vector3.zero)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + speedIncreaseRate * Time.deltaTime, 0, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed - speedDecreaseRate * Time.deltaTime, 0, maxSpeed);
        }

        // 이동 적용
        transform.position += currentDirection.normalized * currentSpeed * Time.deltaTime;

        // 회전 적용
        if (currentDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            animator.SetFloat("X", horizontal);
            animator.SetFloat("Y", vertical);
            animator.SetFloat("Speed", currentSpeed);
            animator.SetBool("OnGround", true);

            // Root Motion 제거
            animator.applyRootMotion = false;
        }
    }
}
