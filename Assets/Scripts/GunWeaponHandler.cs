using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunWeaponHandler : MonoBehaviour
{
    public Rigidbody p_rb;
    public Transform gunPos;
    public Transform target;

    public RectTransform crosshair_;
    public Camera mainCam;

    public float bulletSpeed;
    
    private ParticleSystem bulletParticle;
    private Vector3 interceptPosition = Vector3.zero;


    private void Start()
    {
        
        bulletParticle = transform.GetComponent<ParticleSystem>();
        bulletParticle.Play();
    }

    public void fireBullet()
    {
        bulletParticle.enableEmission = true;
        var fireVel = p_rb.transform.forward * bulletSpeed;

        ParticleSystem.VelocityOverLifetimeModule velOverLifeTime = bulletParticle.velocityOverLifetime;

        velOverLifeTime.x = fireVel.x;
        velOverLifeTime.y = fireVel.y;
        velOverLifeTime.z = fireVel.z;

        

    }




    void Update()
    {
        transform.position = gunPos.position;

        if (Input.GetKey(KeyCode.Mouse0))
        {
            fireBullet();
        }
        else
        {
            bulletParticle.enableEmission = false;
        }

        //crosshair position getter

        Vector3 bullet_Pos = gunPos.position + (p_rb.transform.forward * (bulletSpeed * bulletParticle.startLifetime));


        Vector2 crosshair_Pos = mainCam.WorldToScreenPoint(bullet_Pos);

        crosshair_.position = crosshair_Pos;




    }
    private void OnParticleCollision(GameObject other)
    {
        if(other.transform.tag == "Enemy")
        {
            print("enemy has been hit by your machine guns");
        }
    }
}
