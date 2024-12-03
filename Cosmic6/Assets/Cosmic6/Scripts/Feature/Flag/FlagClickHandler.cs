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
            Debug.LogError("FlagState 스크립트를 가진 오브젝트가 없습니다!");
        }
        instantMovement = FindObjectOfType<InstantMovement>();
        if (flagState == null)
        {
            Debug.LogError("FlagState 스크립트를 가진 오브젝트가 없습니다!");
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
                        Debug.Log("이미 등록된 Flag입니다.");
                    }
                    else
                    {
                        Debug.Log("Flag1 클릭됨!");
                        flagState.flag1_Registered = true;
                        Debug.Log("flag1이 등록 상태로 변경되었습니다.");

                        flag1.SetActive(false);
                    }
                }
                else if (flag2 != null && hit.collider.gameObject == flag2)
                {
                    if (flagState.flag2_Registered == true)
                    {
                        Debug.Log("이미 등록된 Flag입니다.");
                    }
                    else
                    {
                        Debug.Log("Flag2 클릭됨!");
                        flagState.flag2_Registered = true;
                        Debug.Log("flag2가 등록 상태로 변경되었습니다.");

                        flag2.SetActive(false);
                    }
                }
                else if (flag3 != null && hit.collider.gameObject == flag3)
                {
                    if (flagState.flag3_Registered == true)
                    {
                        Debug.Log("이미 등록된 Flag입니다.");
                    }
                    else
                    {
                        Debug.Log("Flag3 클릭됨!");
                        flagState.flag3_Registered = true;
                        Debug.Log("flag3이 등록 상태로 변경되었습니다.");

                        flag3.SetActive(false);
                    }
                }
                else if (flag4 != null && hit.collider.gameObject == flag4)
                {
                    if (flagState.flag4_Registered == true)
                    {
                        Debug.Log("이미 등록된 Flag입니다.");
                    }
                    else
                    {
                        Debug.Log("Flag4 클릭됨!");
                        flagState.flag4_Registered = true;
                        Debug.Log("flag4가 등록 상태로 변경되었습니다.");

                        flag4.SetActive(false);
                    }
                }
                else if (flag5 != null && hit.collider.gameObject == flag5)
                {
                    if (flagState.flag5_Registered == true)
                    {
                        Debug.Log("이미 등록된 Flag입니다.");
                    }
                    else
                    {
                        Debug.Log("Flag5 클릭됨!");
                        flagState.flag5_Registered = true;
                        Debug.Log("flag5가 등록 상태로 변경되었습니다.");

                        flag5.SetActive(false);

                        instantMovement.enabled = true;
                    }
                }
            }
        }
    }
}
