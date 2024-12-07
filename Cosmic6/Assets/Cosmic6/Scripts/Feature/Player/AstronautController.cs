using UnityEngine;
using DevionGames;

public class AstronautController : ThirdPersonController
{
    public float liftSpeed = 5f; // 우주선 상승 속도
    public float descendSpeed = 5f; // 우주선 하강 속도

    protected override void Update()
    {
        base.Update(); // 기존 ThirdPersonController 이동 로직

        // 상승 및 하강 제어
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * liftSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position -= Vector3.up * descendSpeed * Time.deltaTime;
        }
    }
}
