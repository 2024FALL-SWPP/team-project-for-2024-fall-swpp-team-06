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
            fullmapCamera.gameObject.SetActive(false);

        foreach (var icon in baseIcons)
            icon.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerTransform != null)
            playerIcon.transform.position = playerIcon.transform.parent.position;

        if (isFullMap)
            playerIcon.transform.rotation = Quaternion.Euler(90, 30, 30);
        else
            playerIcon.transform.localRotation = Quaternion.Euler(90, 30, 120);

        if (Input.GetKeyDown(KeyCode.Escape) && isFullMap)
        {
            CloseFullMap();
        }
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
        playerIcon.transform.localScale = new Vector3(200, 200, 1);
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
        playerIcon.transform.localScale = new Vector3(5, 5, 1);
        SetBaseIconScale(new Vector3(3, 3, 1));
    }

    public bool IsFullMap()
    {
        return isFullMap;
    }

    private void SetBaseIconScale(Vector3 desiredWorldScale)
    {
        foreach (var icon in baseIcons)
        {
            Vector3 parentScale = icon.transform.parent != null
                ? icon.transform.parent.localScale
                : Vector3.one;

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
                if (i == 0)
                    baseIcons[0].gameObject.SetActive(true);
                else if (i == 1)
                    baseIcons[1].gameObject.SetActive(true);
                else
                    baseIcons[2].gameObject.SetActive(true);
            }
        }
    }
}
