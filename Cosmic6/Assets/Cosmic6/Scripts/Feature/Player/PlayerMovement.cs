using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    public GameManager gameManager;
    public InstantMovement instantMovement;
    public float groundedSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -4.9f;
    public float airMovementFactor = 0.5f;

    private LocationTracker locationTracker;
    private bool isGrounded = true;
    private Vector3 displacement;
    private Vector3 velocity;
    private CharacterController characterController;
    private float characterControllerHeight = 0.84f;
    private Animator animator;
    private bool isJumping = false;
    
    // Start is called before the first frame update
    void Start()
    {
        locationTracker = GetComponent<LocationTracker>();
        gameManager.OnGameOver += GameOver;
        instantMovement.OnTeleport += Teleport;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        if (isGrounded && isJumping)
        {
            isJumping = false;
            animator.SetBool("Jump_b", false);
        }
        
        
        if (!gameManager.isGameOver)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
        
            animator.SetFloat("Horizontal", moveX);
            animator.SetFloat("Vertical", moveZ);
        
            float speed = isGrounded ? groundedSpeed : groundedSpeed * airMovementFactor;
        
            displacement = transform.right * moveX + transform.forward * moveZ;
            characterController.Move(speed * Time.deltaTime * displacement);
        }
        else
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
        }
            
        
        if (!gameManager.isGameOver && Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
            isJumping = true;
            animator.SetBool("Jump_b", true);
        }
        
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void GameOver()
    {
        int idx = Random.Range(0, 2);
        animator.SetTrigger(idx == 0 ? "DyeTrig" : "DyeBackwardsTrig");
        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        float changeSpeed = -0.1f;
        float timer = 0;
        float acceleration = 0.15f * 5 / gameManager.gameOverAnimationDuration;
        
        while (timer < gameManager.gameOverAnimationDuration / 2)
        {
            timer += Time.deltaTime;
            characterController.height += changeSpeed * Time.deltaTime;
            changeSpeed -= acceleration * Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(gameManager.gameOverAnimationDuration / 2);
        characterController.height = characterControllerHeight;
        characterController.enabled = false;
        transform.position = locationTracker.respawnLocations[locationTracker.lastRespawnIndex];
        characterController.enabled = true;
    }

    void Teleport()
    {
        Vector3 teleportPosition = instantMovement.basePositions[instantMovement.targetBaseIndex].transform.position;
        StartCoroutine(TeleportCoroutine(teleportPosition));
    }

    IEnumerator TeleportCoroutine(Vector3 teleportPosition)
    {
        yield return new WaitForSeconds(gameManager.gameOverAnimationDuration / 2);
        characterController.enabled = false;
        transform.position = teleportPosition;
        Debug.Log($"Teleported to Base {instantMovement.targetBaseIndex + 1} at position {teleportPosition}.");
        characterController.enabled = true;
    }
}
