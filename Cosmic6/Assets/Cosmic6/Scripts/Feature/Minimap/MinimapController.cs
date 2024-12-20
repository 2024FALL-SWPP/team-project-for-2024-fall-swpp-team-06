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
        SetBaseIconScale(new Vector3(10, 10, 1));
    }

    void Update()
    {
        
        minimapCamera.transform.position = 
            new (playerTransform.position.x, minimapCamera.transform.position.y, playerTransform.position.z);
        
    }

    public void ShowFullMap()
    {
        minimapCamera.gameObject.SetActive(false);
        fullmapCamera.gameObject.SetActive(true);

        // makes minimap invisible by makes alpha 0f
        
        minimapRawImage.color = new Color(
            minimapRawImage.color.r,
            minimapRawImage.color.g,
            minimapRawImage.color.b,
            0f
        );


        isFullMap = true;
        playerIcon.transform.localScale = new Vector3(150, 150, 1);
        SetBaseIconScale(new Vector3(50, 50, 1));
    }

    public void CloseFullMap()
    {
        
        fullmapCamera.gameObject.SetActive(false);
        minimapCamera.gameObject.SetActive(true);
        
        minimapRawImage.color = new Color(
            minimapRawImage.color.r,
            minimapRawImage.color.g,
            minimapRawImage.color.b,
            1f
        );

        isFullMap = false;
        playerIcon.transform.localScale = new Vector3(18f, 18f, 1f);
        SetBaseIconScale(new Vector3(10, 10, 1));
    }

    private void SetBaseIconScale(Vector3 desiredWorldScale)
    {
        foreach (var icon in baseIcons)
        {
            // parentScale 계산
            Vector3 parentScale = icon.transform.parent.localScale;

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
