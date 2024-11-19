using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    private Button closeButton;

    private void Start()
    {
        closeButton = transform.Find("Button").GetComponent<Button>();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    private void ClosePopup()
    {
        Destroy(gameObject);
    }
}
