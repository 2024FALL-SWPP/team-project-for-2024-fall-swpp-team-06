using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameManager gameManager;
    
    public float horizontalSensitivity = 150f;
    public float verticalSensitivity = 100f;
    public float xRotationLimit = 89f;

    public GameObject player;
    private Transform playerTransform;
    
    public Transform headRigTransform;
    private Quaternion headRigRotation = Quaternion.Euler(-94, 90, 0);
    
    public Transform headTransform;
    private Vector3 headPosition = new (0, -0.17f, -0.03f);
    
    private float xRotation = 0f;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager.OnGameOver += GameOver;
        Cursor.lockState = CursorLockMode.Locked;
        playerTransform = player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.IsGameOver) return;
        
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.deltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerTransform.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(90 + xRotation, 0, 0f);
    }

    void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        transform.SetParent(headRigTransform, true);
        
        
        
        transform.localRotation = headRigRotation;
        
        yield return new WaitForSeconds(gameManager.gameOverAnimationDuration);
        
        transform.SetParent(headTransform, true);
        transform.localPosition = headPosition;
        transform.localRotation = Quaternion.Euler(90 + xRotation, 0, 0f);
    }
}
