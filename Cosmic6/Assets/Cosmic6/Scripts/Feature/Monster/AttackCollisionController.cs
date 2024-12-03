using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class AttackCollisionController : MonoBehaviour
{
    //public delegate void OnHitDelegate(Collider other, GameObject hitPart);
    public event Action<Collider, GameObject> OnHit;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTriggerEnter(Collider other)
    {
        OnHit?.Invoke(other, gameObject);
    }
}
