using System;
using RayFire;
using UnityEngine;
using UnityEngine.VFX;
public class PlayerShoot : MonoBehaviour
{
    public VisualEffect gunEffect;
    RayfireBomb barril;
    [SerializeField] RayfireGun rayfireGun;
    public PlayerWeapon weapon;
    [SerializeField] Camera cam;
    [SerializeField] LayerMask mask;
    public AudioSource gun;

    private void Start()
    {
        gunEffect.Stop();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            shot();
        }
    }

    private void shot()
    {
        gunEffect.Play();
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        rayfireGun.Shoot();
        gun.Play();
        if (Physics.Raycast(ray, out RaycastHit hit, weapon.range, mask))
        {
            Debug.Log("Objet touché : " + hit.collider.name);
            if (hit.collider.name == "GO_barril")
            {
                RayfireBomb barrel = hit.collider.GetComponent<RayfireBomb>();
                barrel.Explode(0);
            }
        }
    }
}
