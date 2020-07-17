using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PineTree : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButton(0) && this.gameObject.activeInHierarchy)
        {
            Destroy(this.gameObject);
        }
    }
}
