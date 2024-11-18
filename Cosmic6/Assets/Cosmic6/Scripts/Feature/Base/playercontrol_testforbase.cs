using UnityEngine;

public class playercontrol_testforbase : MonoBehaviour
{
    public float speed = 5.0f; // 이동 속도
    public float rotationSpeed = 720.0f; // 회전 속도
    private Rigidbody rb;

    void Start()
    {
        // Rigidbody 초기화
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody 컴포넌트가 없습니다. Rigidbody를 추가하세요.");
        }
    }

    void Update()
    {
        if (rb == null) return; // Rigidbody가 없으면 실행 중지

        // 입력 값 가져오기
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 이동 방향 계산
        Vector3 movement = new Vector3(horizontal, 0.0f, vertical).normalized;

        // 이동 처리
        if (movement.magnitude > 0.1f)
        {
            // 이동 방향으로 회전
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            // 이동
            rb.MovePosition(transform.position + movement * speed * Time.deltaTime);
        }
    }
}