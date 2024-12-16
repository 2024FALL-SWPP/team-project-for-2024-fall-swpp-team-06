using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public Camera fullmapCamera;
    private bool isFullMap = false;

    public SpriteRenderer playerIcon;
    public SpriteRenderer[] baseIcons;
    public RawImage minimapRawImage;

    public Transform playerTransform;
    public BaseManager baseManager;

    void Start()
    {
        if (fullmapCamera != null)
        {
            fullmapCamera.gameObject.SetActive(false);
        }
        foreach (var icon in baseIcons)
        {
            icon.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        minimapCamera.transform.position = new(playerTransform.position.x, minimapCamera.transform.position.y, playerTransform.position.z);
        // 플레이어 아이콘 추적 로직 (필요하다면 유지)
        if (playerTransform != null && playerIcon != null)
        {
            Vector3 pos = playerIcon.transform.parent.position;
            playerIcon.transform.position = new Vector3(playerTransform.position.x, pos.y, playerTransform.position.z);
        }

        // 여기서 M키 입력 제거
        // M키 입력은 UIManager에서 처리하므로 MinimapController에서는 하지 않는다.

        // ESC로 풀맵 종료 로직도 제거하거나 필요하면 유지
        // 하지만 UIManager에서 상태 전환 관리 시 ESC를 UIManager에서 처리 권장
    }

    public void ShowFullMap()
    {
        if (fullmapCamera != null)
        {
            minimapCamera.gameObject.SetActive(false);
            fullmapCamera.gameObject.SetActive(true);
        }

        if (minimapRawImage != null)
        {
            minimapRawImage.color = new Color(
                minimapRawImage.color.r,
                minimapRawImage.color.g,
                minimapRawImage.color.b,
                0f
            );
        }

        isFullMap = true;
        if (playerIcon != null) playerIcon.transform.localScale = new Vector3(150, 150, 1);
        SetBaseIconScale(new Vector3(50, 50, 1));
    }

    public void CloseFullMap()
    {
        if (fullmapCamera != null)
        {
            fullmapCamera.gameObject.SetActive(false);
            minimapCamera.gameObject.SetActive(true);
        }

        if (minimapRawImage != null)
        {
            minimapRawImage.color = new Color(
                minimapRawImage.color.r,
                minimapRawImage.color.g,
                minimapRawImage.color.b,
                1f
            );
        }

        isFullMap = false;
        if (playerIcon != null) playerIcon.transform.localScale = new Vector3(12.5f, 12.5f, 1f);
        SetBaseIconScale(new Vector3(3, 3, 1));
    }

    private void SetBaseIconScale(Vector3 desiredWorldScale)
    {
        foreach (var icon in baseIcons)
        {
            // parentScale 계산
            Vector3 parentScale = icon.transform.parent != null ? icon.transform.parent.localScale : Vector3.one;

            Vector3 correctedLocalScale = new Vector3(
                desiredWorldScale.x / parentScale.x,
                desiredWorldScale.y / parentScale.y,
                desiredWorldScale.z / parentScale.z
            );

            icon.transform.localScale = correctedLocalScale;
        }
    }

    public void UpdateMinimap()
    {
        for (int i = 0; i < baseManager.isBaseRegistered.Length; i++)
        {
            if (baseManager.isBaseRegistered[i])
            {
                if (i < baseIcons.Length)
                {
                    baseIcons[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
