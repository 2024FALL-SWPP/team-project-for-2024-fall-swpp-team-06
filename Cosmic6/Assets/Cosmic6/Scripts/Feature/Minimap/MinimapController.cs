using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public Camera fullmapCamera;
    private bool isFullMap = false;

    public SpriteRenderer playerIcon;
    public SpriteRenderer[] baseIcons;
    public RawImage minimapRawImage;
    //public SpriteRenderer[] regionSprites;

    //public PlayerState playerState;

    public Transform playerTransform;
    public BaseManager baseManager;
    
    private RawImage rawImage;


    // Start is called before the first frame update
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        if (fullmapCamera != null)
        {
            fullmapCamera.gameObject.SetActive(false);
        }
        foreach (var icon in baseIcons)
        {
            icon.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateRegionVisibility();
        
        minimapCamera.transform.position = 
            new (playerTransform.position.x, minimapCamera.transform.position.y, playerTransform.position.z);

    /*
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isFullMap)
            {
                CloseFullMap();
            }
            else
            {
                ShowFullMap();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isFullMap)
        {
            CloseFullMap();
        }*/
    }

    /*void UpdateRegionVisibility()
    {
        if (playerState == null || regionSprites == null || regionSprites.Length < 3) return;

        regionSprites[0].enabled = playerState.isRegistered1;   // Region1
        regionSprites[1].enabled = playerState.isRegistered2;   // Region2
        regionSprites[2].enabled = playerState.isRegistered3;   // Region3
    }*/

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
        

        // makes minimap visible by makes alpha 1.0f
        
        minimapRawImage.color = new Color(
            minimapRawImage.color.r,
            minimapRawImage.color.g,
            minimapRawImage.color.b,
            1f
        );
        

        isFullMap = false;
        playerIcon.transform.localScale = new Vector3(12.5f, 12.5f, 1f);
        SetBaseIconScale(new Vector3(3, 3, 1));
    }

    private void SetBaseIconScale(Vector3 desiredWorldScale)
    {

        foreach (var icon in baseIcons)
        {
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
                if (i == 0)
                {
                    baseIcons[0].gameObject.SetActive(true);
                }
                else if (i == 1)
                {
                    baseIcons[1].gameObject.SetActive(true);
                }
                else
                {
                    baseIcons[2].gameObject.SetActive(true);
                }
            }
        }
    }
}
