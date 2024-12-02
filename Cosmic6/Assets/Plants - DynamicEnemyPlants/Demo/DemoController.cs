using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoController : MonoBehaviour
{
    public Transform[] plant;
    public Transform[] cam;
    public bool[] isOnPlant;
    public GameObject sporePlant1, sporePlant2, sporePlant31, sporePlant32, sporePlant33, sporePlant34;
    private Animator animator;
    public SimpleRotate simpleRotateR;
    public SimpleRotate simpleRotateL;
    private bool lightOn = true;
    public Transform camLookAt;
    private Light lt;
    private bool idleReadyOn;
    private int plantSelectedID;

    private void Start()
    {
        lt = GameObject.Find("/DirectionalLight").GetComponent<Light>();
    }

    private void Update()
    {
        isOnPlant[plantSelectedID] = true;
        if (animator == null)
        {
            animator = plant[0].gameObject.GetComponentInChildren<Animator>();
        }
        cam[plantSelectedID].Translate(Vector3.right * Time.deltaTime);
        cam[plantSelectedID].LookAt(camLookAt);

        if (lightOn)
        {
            lt.intensity = 1.5f;
        }
        else 
        {
            lt.intensity = 0.1f;
        }

        if (!idleReadyOn)
        {
            animator.SetFloat("IdleStance", 0f, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("IdleStance", 1f, 0.1f, Time.deltaTime);
        }
    }

    public void ToggleLight()
    {
        lightOn = !lightOn;
    }

    //Choosing plant view

    public void SetPlantCamera(int plantID)
    {
        DisableSpores();
        plantSelectedID = plantID;
        isOnPlant[0] = false;
        isOnPlant[1] = false;
        isOnPlant[2] = false;
        isOnPlant[3] = false;
        simpleRotateR.target = plant[plantSelectedID];
        simpleRotateL.target = plant[plantSelectedID];
        animator = plant[plantSelectedID].gameObject.GetComponentInChildren<Animator>();
        foreach (Transform cam in cam)
        {
            if(cam != this.cam[plantSelectedID])
            {
                cam.gameObject.SetActive(false);
            }
            else
            {
                cam.gameObject.SetActive(true);
            }
        }
    }

    public void DisableSpores()
    {
        sporePlant1.SetActive(false);
        sporePlant2.SetActive(false);
        sporePlant31.SetActive(false);
        sporePlant32.SetActive(false);
        sporePlant33.SetActive(false);
        sporePlant34.SetActive(false);
    }

    //Animations

    public void PlayIdle()
    {
        idleReadyOn = false;
        animator.SetTrigger("Idle");
        animator.SetFloat("IdleStance", 0f, 1, Time.deltaTime);
        DisableSpores();
    }

    public void PlayIdleReady()
    {
        idleReadyOn = true;
        animator.SetTrigger("Idle");
        animator.SetFloat("IdleStance", 1f, 1, Time.deltaTime);
        DisableSpores();
    }

    public void PlayMeleeAttack()
    {
        animator.SetTrigger("MeleeAttack");
        DisableSpores();
    }

    public void PlayRangeAttack()
    {
        animator.SetTrigger("RangeAttack");
        DisableSpores();
    }

    public void PlaySpell()
    {
        animator.SetTrigger("Spell");
        if (isOnPlant[0])
        {
            sporePlant1.SetActive(true);
            sporePlant2.SetActive(false);
            sporePlant31.SetActive(false);
            sporePlant32.SetActive(false);
            sporePlant33.SetActive(false);
            sporePlant34.SetActive(false);
        }
        if (isOnPlant[1])
        {
            sporePlant1.SetActive(false);
            sporePlant2.SetActive(true);
            sporePlant31.SetActive(false);
            sporePlant32.SetActive(false);
            sporePlant33.SetActive(false);
            sporePlant34.SetActive(false);
        }
        if (isOnPlant[2])
        {
            sporePlant1.SetActive(false);
            sporePlant2.SetActive(false);
            sporePlant31.SetActive(true);
            sporePlant32.SetActive(true);
            sporePlant33.SetActive(true);
            sporePlant34.SetActive(true);
        }
        if (isOnPlant[3])
        {
            sporePlant1.SetActive(false);
            sporePlant2.SetActive(false);
            sporePlant31.SetActive(false);
            sporePlant32.SetActive(false);
            sporePlant33.SetActive(false);
            sporePlant34.SetActive(false);
        }

    }

    public void PlayDeath()
    {
        animator.SetTrigger("Death");
        DisableSpores();
    }
}
