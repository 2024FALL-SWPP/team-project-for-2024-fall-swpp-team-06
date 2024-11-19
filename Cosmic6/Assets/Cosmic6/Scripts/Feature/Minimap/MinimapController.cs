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
        if (Input.GetMouseButtonDown(0) && !isFullMap)
        {
            RaycastHit hit;
            Ray ray = minimapCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                ShowFullMap();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isFullMap)
        {
            CloseFullMap();
        }
    }

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
