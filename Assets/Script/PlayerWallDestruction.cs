using System;
using UnityEngine;

public class RayfireBreaker : MonoBehaviour
{
    private BoxCollider boxCollider;


    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider boxCollider)
    {
        if (boxCollider.gameObject.layer == LayerMask.NameToLayer("Destructible"))
        {
            
        }
    }
}
