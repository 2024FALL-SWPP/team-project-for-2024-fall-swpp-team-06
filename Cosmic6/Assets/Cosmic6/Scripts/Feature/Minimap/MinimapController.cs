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
    //public SpriteRenderer[] regionSprites;

    //public PlayerState playerState;

    public Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (fullmapCamera != null)
        {
            fullmapCamera.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

        //UpdateRegionVisibility();

        if (playerTransform != null)
        {
            playerIcon.transform.position = playerIcon.transform.parent.position;
        }

        if (isFullMap)
        {
            playerIcon.transform.rotation = Quaternion.Euler(90, 30, 30);
        }
        else
        {
            playerIcon.transform.localRotation = Quaternion.Euler(90, 30, 120);
        }

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
            /*RaycastHit hit;
            Ray ray = minimapCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Minimap"))
                {
                    ShowFullMap();
                }
            }*/
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isFullMap)
        {
            CloseFullMap();
        }
    }

    /*void UpdateRegionVisibility()
    {
        if (playerState == null || regionSprites == null || regionSprites.Length < 3) return;

        regionSprites[0].enabled = playerState.isRegistered1;   // Region1
        regionSprites[1].enabled = playerState.isRegistered2;   // Region2
        regionSprites[2].enabled = playerState.isRegistered3;   // Region3
    }*/

    void ShowFullMap()
    {
        if (fullmapCamera != null)
        {
            minimapCamera.gameObject.SetActive(false);
            fullmapCamera.gameObject.SetActive(true);
        }
        isFullMap = true;

        playerIcon.transform.localScale = new Vector3(300, 300, 1);
        foreach (var icon in baseIcons)
        {
            icon.transform.localScale = new Vector3(10, 10, 1);
        }
    }

    public void CloseFullMap()
    {
        if (fullmapCamera != null)
        {
            fullmapCamera.gameObject.SetActive(false);
            minimapCamera.gameObject.SetActive(true);
        }
        isFullMap = false;

        playerIcon.transform.localScale = new Vector3(5, 5, 1);
        foreach (var icon in baseIcons)
        {
            icon.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
