/* -------------------- 
 This code takes heavy influence from the video:
 
 https://www.youtube.com/watch?v=THnivyG0Mvo 

-------------------- */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWeapon : MonoBehaviour
{

    public float damage = 10;
    public float range = 100f;

    public Camera fpsCam;
    public ParticleSystem MuzzleFlash;
    public GameObject ImpactEffect;
    public GameObject CrossHair;
    public int forceMultiplier = 1;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ShootWeapon();
        }

        if (Input.GetButton("Fire2"))
        {
            AimWeapon();
        }

        // Return weapon to hip and camera to original FOV (interpolated)
        if (!Input.GetButton("Fire2"))
        {
            gameObject.transform.localPosition = new Vector3 
                (Mathf.Lerp(gameObject.transform.localPosition.x, 0.5f, .03f),
                Mathf.Lerp(gameObject.transform.localPosition.y, -0.4f, .03f), 
                Mathf.Lerp(gameObject.transform.localPosition.z, 0.7f, .03f));
            
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, .01f);
            CrossHair.SetActive(true);
        }

    }

    void ShootWeapon() 
    {
        RaycastHit hit;

        // Play the muzzleflash
        MuzzleFlash.Play();

        // Send out a Raycast to represent a bullet
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            // Get the target that that was hit (null if no target component)
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }

        // Add a force in the opposite direction of the normal (if rigidbody exists)
        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForce(-hit.normal * forceMultiplier);
        }

        // Create the impact effect on the normal of the impact point
        GameObject impactGameObject = Instantiate(ImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(impactGameObject, 2f);
    }

    /* 
        Improvements:
        - Interpolated the distance traveled while aiming
        - Zoomed in the weapon -> changing camera FOV
    */
    void AimWeapon()
    {
        // Transforms the weapon to the center of the screen
        CrossHair.SetActive(false);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 40, .01f);
        Debug.Log($"Aiming Weapon: {Camera.main.fieldOfView}");

        gameObject.transform.localPosition = new Vector3 
            (Mathf.Lerp(gameObject.transform.localPosition.x, 0.0025f, .12f),
            Mathf.Lerp(gameObject.transform.localPosition.y, -0.23f, .12f), 
            Mathf.Lerp(gameObject.transform.localPosition.z, 0.636f, .12f));
            
    }
}
