using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAbs : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && this.gameObject.activeInHierarchy)
        {
            Destroy(this.gameObject);
        }
    }
}
