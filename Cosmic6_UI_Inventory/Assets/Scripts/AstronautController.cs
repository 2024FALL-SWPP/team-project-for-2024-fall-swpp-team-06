using UnityEngine;

public class AstronautController : MonoBehaviour
{
    public float maxSpeed = 1f;              // 최대 속력
    public float speedIncreaseRate = 0.3f;   // 속도 증가율
    public float speedDecreaseRate = 2f;   // 속도 감소율
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

        // 카메라 forward, right의 y값을 0으로 평탄화(수평면 기준 이동)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 방향키 입력 조합 처리
        Vector3 inputDirection = Vector3.zero;

        // 서로 반대되는 입력 처리
        bool verticalConflict = up && down;
        bool horizontalConflict = left && right;

        if (!verticalConflict && !horizontalConflict)
        {
            // 위/아래/왼/오른 조합에 따라 desiredDirection 결정
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
            // 위+아래 또는 왼+오른 동시 입력: 입력 무시, 현재 방향 유지
            inputDirection = Vector3.zero;
        }

        // 회전 처리
        // inputDirection이 0이 아니면 desiredDirection 업데이트
        // 0이면 현재 방향 유지 (단, 이동은 점차 감소)
        if (inputDirection != Vector3.zero)
        {
            desiredDirection = inputDirection;
        }
        else
        {
            // 입력이 전혀 없는 경우 방향 유지
            // desiredDirection 변경 없음
        }

        // 현재 바라보는 방향(currentDirection)에서 desiredDirection으로 스무스하게 회전
        if (desiredDirection != Vector3.zero && currentDirection != Vector3.zero)
        {
            // 회전할 각도 계산
            float angle = Vector3.SignedAngle(currentDirection, desiredDirection, Vector3.up);
            // 각도가 0이 아닐 때 회전 진행
            if (Mathf.Abs(angle) > rotationThreshold)
            {
                // 회전 방향 결정
                // 반시계 회전 소요시간 vs 시계 회전 소요시간을 비교하려면
                // 여기서는 단순히 angle 부호로 판단
                // tie (정확히 180도) 인 경우 angle이 양수가 되도록 하여 시계방향(정해진 규칙) 택할 수 있도록 약간의 보정
                if (Mathf.Abs(angle) == 180f)
                {
                    // 180도인 경우 시계 방향 선택 -> angle 음수로 만들기
                    // 시계/반시계 정의는 y축 기준
                    // angle > 0이면 반시계, angle < 0이면 시계
                    // 여기서는 tie일 경우 시계방향이라고 했으므로 angle을 음수로 만들어준다.
                    angle = -angle;
                }

                float rotateAngle = Mathf.MoveTowardsAngle(0, angle, rotationSpeed * Time.deltaTime * Mathf.Abs(angle));
                // rotateAngle만큼 회전 적용
                Quaternion rot = Quaternion.AngleAxis(rotateAngle, Vector3.up);
                currentDirection = rot * currentDirection;
            }
        }

        // 속도 처리
        // desiredDirection과 currentDirection이 있을 때,
        // 입력이 있다면 속도 증가, 없다면 감소
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

        // 실제 오브젝트 회전 갱신
        if (currentDirection != Vector3.zero)
        {
            // 현재 오브젝트 forward를 currentDirection로 천천히 맞춰준다.
            // currentDirection는 이미 y축 회전만 고려. 오브젝트 수직 방향 유지 가정
            Quaternion targetRot = Quaternion.LookRotation(currentDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            // Animator 파라미터 갱신
            // 현재 Animator에서 X, Y는 Input축 그대로 사용 가능
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
