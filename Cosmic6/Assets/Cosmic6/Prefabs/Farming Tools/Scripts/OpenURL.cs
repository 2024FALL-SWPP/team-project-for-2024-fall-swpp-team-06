using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StylizedTools
{
    public class OpenURL : MonoBehaviour
    {
        [SerializeField] string url;

        public void _OnURLOpen()
        {
            Application.OpenURL(url);
        }
    }
}
