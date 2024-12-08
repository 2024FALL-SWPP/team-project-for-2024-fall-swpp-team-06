using UnityEngine;

public class AstronautPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementMaxSpeed = 1.5f;             // 이동용 최대 속력
    public float movementIncreaseRate = 1f;         // 이동용 속도 증가율
    public float movementDecreaseRate = 2f;         // 이동용 속도 감소율

    [Header("Animator Settings")]
    public float animatorMaxSpeed = 1f;             // 애니메이터용 최대 속력(시각적)
    public float animatorIncreaseRate = 0.5f;         // 애니메이터 속도 증가율
    public float animatorDecreaseRate = 2f;         // 애니메이터 속도 감소율

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;               // 회전 속도
    public float rotationThreshold = 0.001f;        // 회전 방향 결정 임계값

    [Header("Jump & Gravity")]
    public float jumpHeight = 2f;
    public float gravity = -4.9f;
    public float airMovementFactor = 0.5f;

    private Animator animator;

    // 이동용 속도
    private float movementCurrentSpeed = 0f;
    // 애니메이터용 속도
    private float animatorCurrentSpeed = 0f;

    private Vector3 desiredDirection = Vector3.zero;
    private Vector3 currentDirection = Vector3.forward;

    private Transform mainCamera;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isJumping = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator is missing!");
        }

        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Camera not found!");
        }

        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController is missing! Add a CharacterController component to the Astronaut.");
        }
    }

    private void Update()
    {
        bool isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        HandleMovement(isGrounded);
        UpdateAnimatorParameters(isGrounded);

        // 점프 처리
        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
            isJumping = true;
            animator.SetBool("Jump_b", true);
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // 착지 체크
        if (isGrounded && isJumping)
        {
            isJumping = false;
            animator.SetBool("Jump_b", false);
        }

        // 회전 적용
        if (currentDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleMovement(bool isGrounded)
    {
        // 입력 받기
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // 카메라 기준 방향 벡터
        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

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
                Vector3 backDir = -cameraForward;
                inputDirection = (backDir - cameraRight).normalized;
            }
            else if (down && right && !up && !left)
            {
                Vector3 backDir = -cameraForward;
                inputDirection = (backDir + cameraRight).normalized;
            }
            else
            {
                inputDirection = Vector3.zero;
            }
        }
        else
        {
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
            if (Mathf.Abs(angle) == 180f)
            {
                angle = Mathf.Abs(angle);
            }

            if (Mathf.Abs(angle) > rotationThreshold)
            {
                float rotateAngle = Mathf.MoveTowardsAngle(0, angle, rotationSpeed * Time.deltaTime * Mathf.Abs(angle));
                Quaternion rot = Quaternion.AngleAxis(rotateAngle, Vector3.up);
                currentDirection = rot * currentDirection;
            }
        }

        // 속도 처리(이동용)
        float effectiveMaxSpeed = isGrounded ? movementMaxSpeed : movementMaxSpeed * airMovementFactor;

        if (inputDirection != Vector3.zero)
        {
            movementCurrentSpeed = Mathf.Clamp(movementCurrentSpeed + movementIncreaseRate * Time.deltaTime, 0, effectiveMaxSpeed);
        }
        else
        {
            movementCurrentSpeed = Mathf.Clamp(movementCurrentSpeed - movementDecreaseRate * Time.deltaTime, 0, effectiveMaxSpeed);
        }

        // 애니메이터용 속도 계산
        // 이동 speed 대비 비율 계산
        float fraction = (movementMaxSpeed > 0f) ? (movementCurrentSpeed / movementMaxSpeed) : 0f;
        fraction = Mathf.Clamp01(fraction); // 분수값 0~1

        // fraction을 기반으로 animatorTargetSpeed 계산
        float animatorTargetSpeed = fraction * animatorMaxSpeed;

        // animatorCurrentSpeed를 animatorTargetSpeed로 부드럽게 이동
        if (animatorCurrentSpeed < animatorTargetSpeed)
        {
            // 증가 방향
            animatorCurrentSpeed = Mathf.Clamp(animatorCurrentSpeed + animatorIncreaseRate * Time.deltaTime, 0, animatorTargetSpeed);
        }
        else
        {
            // 감소 방향
            animatorCurrentSpeed = Mathf.Clamp(animatorCurrentSpeed - animatorDecreaseRate * Time.deltaTime, animatorTargetSpeed, animatorCurrentSpeed);
        }

        // 이동 적용
        Vector3 move = currentDirection.normalized * movementCurrentSpeed * Time.deltaTime;
        characterController.Move(move);
    }

    private void UpdateAnimatorParameters(bool isGrounded)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", animatorCurrentSpeed); // animator에는 animatorCurrentSpeed를 전달
            animator.SetBool("OnGround", isGrounded);
            animator.applyRootMotion = false;
        }
    }
}
