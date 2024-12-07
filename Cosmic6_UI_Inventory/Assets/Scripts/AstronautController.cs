using UnityEngine;

public class AstronautController : MonoBehaviour
{
    public float maxSpeed = 2f; // 최대 Speed 값
    public float speedIncreaseRate = 1f; // Speed 증가 속도
    public float rotationSpeed = 2f; // 회전 속도
    public float fixedY = 0f; // Y 위치 고정 값

    private Animator animator; // Animator 참조
    private float currentSpeed = 0f; // 현재 Speed 값
    private Vector3 moveDirection = Vector3.zero; // 이동 방향

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator is missing from the AstronautController!");
        }
    }

    private void Update()
    {
        HandleMovement();
        UpdateAnimatorParameters();
    }

    private void HandleMovement()
    {
        // 이동 입력
        float horizontal = Input.GetAxis("Horizontal"); // 좌/우
        float vertical = Input.GetAxis("Vertical"); // 앞/뒤

        // 전/후 이동 처리
        if (vertical != 0)
        {
            // Speed 증가 (키를 계속 누르고 있을 때)
            currentSpeed = Mathf.Clamp(currentSpeed + speedIncreaseRate * Time.deltaTime, 0, maxSpeed);

            // 이동 방향 계산 (전/후)
            moveDirection = transform.forward * vertical * currentSpeed;
        }
        else
        {
            // Speed 감소
            currentSpeed = Mathf.Clamp(currentSpeed - speedIncreaseRate * Time.deltaTime, 0, maxSpeed);

            // 이동 방향 초기화
            moveDirection = Vector3.zero;
        }

        // 좌/우 방향 변경만 처리 (Speed에는 영향을 미치지 않음)
        if (horizontal != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward + transform.right * horizontal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Transform 이동
        if (currentSpeed < 0.01f) // Speed가 거의 0일 때
        {
            moveDirection = Vector3.zero;
            transform.position = Vector3.Lerp(
                transform.position,
                new Vector3(transform.position.x, fixedY, transform.position.z),
                Time.deltaTime
            );
        }
        else
        {
            transform.position += moveDirection * Time.deltaTime;
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            // Animator 파라미터 업데이트
            animator.SetFloat("X", Input.GetAxis("Horizontal")); // 좌/우
            animator.SetFloat("Y", Input.GetAxis("Vertical"));   // 앞/뒤
            animator.SetFloat("Speed", currentSpeed);
            animator.SetBool("OnGround", true); // 항상 땅에 붙어있다고 가정
        }
    }
}
