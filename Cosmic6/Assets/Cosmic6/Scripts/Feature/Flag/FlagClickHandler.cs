using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagClickHandler : MonoBehaviour
{
    public LayerMask clickableLayer;
    private FlagState flagState;
    private InstantMovement instantMovement;

    public float rayLength = 5.0f;

    private GameObject flag1;
    private GameObject flag2;
    private GameObject flag3;
    private GameObject flag4;
    private GameObject flag5;

    void Start()
    {
        flag1 = GameObject.Find("Flag1");
        flag2 = GameObject.Find("Flag2");
        flag3 = GameObject.Find("Flag3");
        flag4 = GameObject.Find("Flag4");
        flag5 = GameObject.Find("Flag5");

        flagState = FindObjectOfType<FlagState>();
        if (flagState == null)
        {
            Debug.LogError("FlagState ��ũ��Ʈ�� ���� ������Ʈ�� �����ϴ�!");
        }
        instantMovement = FindObjectOfType<InstantMovement>();
        if (flagState == null)
        {
            Debug.LogError("FlagState ��ũ��Ʈ�� ���� ������Ʈ�� �����ϴ�!");
        }
        instantMovement.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayLength, clickableLayer))
            {
                if (flag1 != null && hit.collider.transform.IsChildOf(flag1.transform))
                {
                    if (flagState.flag1_Registered == true)
                    {
                        Debug.Log("�̹� ��ϵ� Flag�Դϴ�.");
                    }
                    else
                    {
                        Debug.Log("Flag1 Ŭ����!");
                        flagState.flag1_Registered = true;
                        Debug.Log("flag1�� ��� ���·� ����Ǿ����ϴ�.");

                        flag1.SetActive(false);
                    }
                }
                else if (flag2 != null && hit.collider.gameObject == flag2)
                {
                    if (flagState.flag2_Registered == true)
                    {
                        Debug.Log("�̹� ��ϵ� Flag�Դϴ�.");
                    }
                    else
                    {
                        Debug.Log("Flag2 Ŭ����!");
                        flagState.flag2_Registered = true;
                        Debug.Log("flag2�� ��� ���·� ����Ǿ����ϴ�.");

                        flag2.SetActive(false);
                    }
                }
                else if (flag3 != null && hit.collider.gameObject == flag3)
                {
                    if (flagState.flag3_Registered == true)
                    {
                        Debug.Log("�̹� ��ϵ� Flag�Դϴ�.");
                    }
                    else
                    {
                        Debug.Log("Flag3 Ŭ����!");
                        flagState.flag3_Registered = true;
                        Debug.Log("flag3�� ��� ���·� ����Ǿ����ϴ�.");

                        flag3.SetActive(false);
                    }
                }
                else if (flag4 != null && hit.collider.gameObject == flag4)
                {
                    if (flagState.flag4_Registered == true)
                    {
                        Debug.Log("�̹� ��ϵ� Flag�Դϴ�.");
                    }
                    else
                    {
                        Debug.Log("Flag4 Ŭ����!");
                        flagState.flag4_Registered = true;
                        Debug.Log("flag4�� ��� ���·� ����Ǿ����ϴ�.");

                        flag4.SetActive(false);
                    }
                }
                else if (flag5 != null && hit.collider.gameObject == flag5)
                {
                    if (flagState.flag5_Registered == true)
                    {
                        Debug.Log("�̹� ��ϵ� Flag�Դϴ�.");
                    }
                    else
                    {
                        Debug.Log("Flag5 Ŭ����!");
                        flagState.flag5_Registered = true;
                        Debug.Log("flag5�� ��� ���·� ����Ǿ����ϴ�.");

                        flag5.SetActive(false);

                        instantMovement.enabled = true;
                    }
                }
            }
        }
    }
}
