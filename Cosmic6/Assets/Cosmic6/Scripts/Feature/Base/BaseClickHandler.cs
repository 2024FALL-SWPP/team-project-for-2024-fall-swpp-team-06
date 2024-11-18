using UnityEngine;

public class BaseClickHandler : MonoBehaviour
{
    public LayerMask clickableLayer;
    private PlayerState playerState;

    public float rayLength = 5.0f;

    void Start()
    {
        playerState = FindObjectOfType<PlayerState>();
        if (playerState == null)
        {
            Debug.LogError("PlayerState 스크립트를 가진 오브젝트가 없습니다!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayLength, clickableLayer))
            {

                
                if (hit.collider.CompareTag("Base1"))
                {
                    if(playerState.isRegistered1== true){
                        Debug.Log("이미 등록된 Base입니다.");
                    }
                    else{
                        Debug.Log("Base1 클릭됨!");
                        playerState.isRegistered1 = true;
                        Debug.Log("Base1이 등록 상태로 변경되었습니다.");
                    }
                }

                else if (hit.collider.CompareTag("Base2"))
                {
                    if(playerState.isRegistered2== true){
                        Debug.Log("이미 등록된 Base입니다.");
                    }
                    else{
                        Debug.Log("Base2 클릭됨!");
                        playerState.isRegistered2 = true;
                        Debug.Log("거점 2가 등록 상태로 변경되었습니다.");
                    }
                }

                else if (hit.collider.CompareTag("Base3"))
                {
                    if(playerState.isRegistered3== true){
                        Debug.Log("이미 등록된 Base입니다.");
                    }
                    else{
                        Debug.Log("Base3 클릭됨!");
                        playerState.isRegistered3 = true;
                        Debug.Log("거점 3이 등록 상태로 변경되었습니다.");
                    }
                }
            }
        }
    }
}