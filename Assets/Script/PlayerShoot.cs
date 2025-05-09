using System;
using RayFire;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] RayfireGun rayfireGun;
    public PlayerWeapon weapon;
    [SerializeField] Camera cam;
    [SerializeField] LayerMask mask;
    public AudioSource gun;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            shot();
        }
    }

    private void shot()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        rayfireGun.Shoot();
        gun.Play();
        if (Physics.Raycast(ray, out RaycastHit hit, weapon.range, mask))
        {
            Debug.Log("Objet touché : " + hit.collider.name);
        }
    }
}
