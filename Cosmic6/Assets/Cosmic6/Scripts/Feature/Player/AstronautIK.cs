using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstronautIK : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_LookOffset = new Vector3(0f, 1.5f, 3f);
    [SerializeField]
    private float m_BodyWeight = 0.6f;
    [SerializeField]
    private float m_HeadWeight = 0.2f;
    [SerializeField]
    private float m_EyesWeight = 0.2f;
    [SerializeField]
    private float m_ClampWeight = 0.35f;

    [SerializeField]
    private bool ik = true; // IK 활성/비활성을 위한 플래그(Inspector 조정 가능)

    private float m_Weight = 0f;
    private Transform m_CameraTransform;
    private Transform m_Transform;
    private Animator m_Animator;
    private Vector3 m_AimPosition;

    private void Start()
    {
        m_CameraTransform = Camera.main.transform;
        m_Transform = transform;
        m_Animator = GetComponent<Animator>();

        if (m_CameraTransform == null)
        {
            Debug.LogWarning("No Main Camera found. IK might not work as intended.");
        }
        if (m_Animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject.");
        }
    }

    private void Update()
    {
        if (m_CameraTransform == null || m_Animator == null)
            return;

        // 시선 목표 지점 계산
        float relativeX = m_CameraTransform.InverseTransformPoint(m_Transform.position).x;
        m_AimPosition = m_Transform.position + m_CameraTransform.forward * m_LookOffset.z
                        + Vector3.up * m_LookOffset.y
                        + m_CameraTransform.right * (m_LookOffset.x - relativeX * 2f);

        // 카메라로부터 캐릭터를 향하는 벡터와 캐릭터 전방의 각도
        Vector3 directionToTarget = m_Transform.position - m_CameraTransform.position;
        float angle = Vector3.Angle(m_Transform.forward, directionToTarget);

        // 90도 이내이고 IK 활성화시 Weight를 1로 서서히 증가, 아니라면 감소
        if (Mathf.Abs(angle) < 90 && ik)
        {
            m_Weight = Mathf.Lerp(m_Weight, 1f, Time.deltaTime);
        }
        else
        {
            m_Weight = Mathf.Lerp(m_Weight, 0f, Time.deltaTime * 2f);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (m_Animator == null)
            return;

        if (layerIndex == 0 && ik)
        {
            m_Animator.SetLookAtPosition(m_AimPosition);
            m_Animator.SetLookAtWeight(m_Weight, m_BodyWeight, m_HeadWeight, m_EyesWeight, m_ClampWeight);
        }
    }
}
