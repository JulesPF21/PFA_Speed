using System;
using RayFire;
using UnityEngine;

public class ActivatorActivation : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]
    private float minActivationSpeed = 15f;
    [SerializeField]
    private float damageRadius = 2f;

    [SerializeField] private float detectionDistance;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float linearVelocityMagnitude = rb.linearVelocity.magnitude;
        float distance = Mathf.Max(detectionDistance, linearVelocityMagnitude * Time.fixedDeltaTime);
        if (linearVelocityMagnitude > minActivationSpeed)
        {
            Debug.DrawRay(rb.position, rb.linearVelocity.normalized * distance, Color.yellow);
            
            if (rb.SweepTest(rb.linearVelocity.normalized, out RaycastHit hit, distance))
            {
                if (hit.collider.TryGetComponent(out RayfireRigid rigid))
                {
                    if (rigid.ApplyDamage(linearVelocityMagnitude * 100, hit.point, damageRadius))
                    {
                        hit.collider.enabled = false;
                    }
                }
            }
        }
    }
}
