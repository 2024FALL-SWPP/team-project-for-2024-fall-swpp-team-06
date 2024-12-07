using UnityEngine;

public class AstronautController : MonoBehaviour
{
    public float maxSpeed = 1f; // 최대 Speed 값
    public float speedIncreaseRate = 0.5f; // Speed 증가 속도
    public float rotationSpeed = 0.1f; // 회전 속도

    private Animator animator;
    private float currentSpeed = 0f; // 현재 Speed 값
    private Vector3 moveDirection = Vector3.zero;

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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 전후 이동 처리
        if (vertical != 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + speedIncreaseRate * Time.deltaTime, 0, maxSpeed);
            moveDirection = transform.forward * vertical * currentSpeed;
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed - speedIncreaseRate * Time.deltaTime, 0, maxSpeed);
            moveDirection = Vector3.zero;
        }

        // 좌우 방향키로 방향만 전환
        if (horizontal != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward + transform.right * horizontal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 이동 적용
        transform.position += moveDirection * Time.deltaTime;
    }

    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            animator.SetFloat("X", Input.GetAxis("Horizontal"));
            animator.SetFloat("Y", Input.GetAxis("Vertical"));
            animator.SetFloat("Speed", currentSpeed);
            animator.SetBool("OnGround", true);

            // Root Motion 제거
            animator.applyRootMotion = false;
        }
    }
}
