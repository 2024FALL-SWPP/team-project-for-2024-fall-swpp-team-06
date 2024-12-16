using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    private bool _rotate;
    public bool left;
    public bool right;
    public Transform target;

    void Update()
    {
        if (_rotate && right)
        {
            target.Rotate(new Vector3(0, 10, 0));
        }

        if (_rotate && left)
        {
            target.Rotate(new Vector3(0, -10, 0));
        }
    }

    public void OnPress()
    {
        _rotate = true;
    }

    public void OnRelease()
    {
        _rotate = false;
    }
}
