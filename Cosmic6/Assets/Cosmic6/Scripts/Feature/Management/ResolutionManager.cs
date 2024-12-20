using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    // Start is called before the first frame update
    private readonly (int, int) targetResolution = (2560, 1440);
    
    void Start()
    {
        SetAspectRatio();
    }
    
    public void SetAspectRatio()
    {
        var width = Screen.width;
        var height = Screen.height;
        
        Screen.SetResolution(targetResolution.Item1, targetResolution.Item2, true);
    }
}
