using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    public float horizontalSensitivity = 150f;
    public float verticalSensitivity = 100f;
    public float xRotationLimit = 89f;
    
    private Transform playerTransform;
    private float xRotation = 0f;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerTransform = transform.parent.parent;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.deltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerTransform.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(xRotation + 90, 0f, 0f);
    }
}
