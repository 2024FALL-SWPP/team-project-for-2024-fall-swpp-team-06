using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    
    public float groundedSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -4.9f;
    public float airMovementFactor = 0.5f;
    
    private bool isGrounded = true;
    private Vector3 displacement;
    private Vector3 velocity;
    private CharacterController characterController;
    private Animator animator;
    private bool isJumping = false;
    
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isGrounded && isJumping)
        {
            isJumping = false;
            animator.SetBool("Jump_b", false);
        }
        
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        animator.SetFloat("Horizontal", moveX);
        animator.SetFloat("Vertical", moveZ);
        
        float speed = isGrounded ? groundedSpeed : groundedSpeed * airMovementFactor;
        
        displacement = transform.right * moveX + transform.forward * moveZ;
        
        characterController.Move(speed * Time.deltaTime * displacement);
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
            isJumping = true;
            animator.SetBool("Jump_b", true);
        }
        
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
