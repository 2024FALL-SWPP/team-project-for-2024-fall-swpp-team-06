using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollisionController : MonoBehaviour
{
    public delegate void OnHitDelegate(Collider other, GameObject hitPart);
    public OnHitDelegate onHit;
    
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
        if (onHit != null)
        {
            onHit.Invoke(other, gameObject);
        }
    }
}
