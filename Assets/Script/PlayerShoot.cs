using System;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public PlayerWeapon weapon;
    [SerializeField] Camera cam;
    [SerializeField] LayerMask mask;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, weapon.range, mask))
        {
            Debug.Log("Objet touché : " + hit.collider.name);
        }
    }
}
