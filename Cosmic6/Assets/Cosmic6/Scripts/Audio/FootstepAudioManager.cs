using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepAudioManager : MonoBehaviour
{
    private LocationTracker locationTracker;
    private AudioSource audioSource;
    private CharacterController characterController;
    private PlayerMovement playerMovement;
    
    public int currentRegionIdx = 0;
    public AudioClip[] footstepClips;
    public float maxVolume = 1f;
    private bool isGrounding = true;
    private float reduceTimer = 0f;
    private float reduceTime = 0.2f;
    private float fromReduce;
    
    
    // Start is called before the first frame update
    void Start()
    {
        locationTracker = GetComponent<LocationTracker>();
        audioSource = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        audioSource.clip = footstepClips[currentRegionIdx];
    }

    // Update is called once per frame
    void Update()
    {
        if (!characterController.isGrounded)
        {
            if (isGrounding)
            {
                fromReduce = audioSource.volume;
                reduceTimer = 0;
                isGrounding = false;
            }
            
            audioSource.volume = Mathf.Max(0, Mathf.Lerp(fromReduce, 0, reduceTimer / reduceTime));
            reduceTimer += Time.deltaTime;
        }
        else
        {
            isGrounding = true;
            audioSource.volume = playerMovement.currentSpeed;
        }
        
        
        if (currentRegionIdx != locationTracker.currentRegionIndex)
        {
            currentRegionIdx = locationTracker.currentRegionIndex;
            audioSource.clip = footstepClips[currentRegionIdx];
            audioSource.Play();
        }
    }
}
