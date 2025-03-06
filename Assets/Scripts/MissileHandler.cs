using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MissileHandler : MonoBehaviour
{
    [SerializeField] GameObject explosion;

    [SerializeField] public Rigidbody plane_rb;
    [SerializeField] public Transform target;
    [SerializeField] public Rigidbody target_rb;

    [SerializeField] private Rigidbody missile_rb;

    [SerializeField] private float thrustPower = 1.0f;
    [SerializeField] private float agility = 1.0f;
    [SerializeField] private float detonationRange = 1.0f;
    private Vector3 interceptionPosition = Vector3.zero;

    [SerializeField] float lockAngle;
    private bool hit = false;

    private void Start()
    {
        target_rb = target.GetComponent<Rigidbody>();
    }

    private void calculateTargetPosition()
    {
        
        Vector3 relVel = target_rb.velocity - missile_rb.velocity;
        float dist = Vector3.Distance(target.position, transform.position);
        float t; 

        if (relVel.magnitude == 0)
        {
            t = 0;
        }
        else
        {
            t = dist / relVel.magnitude;
        }
        

        interceptionPosition = target.position + (target_rb.velocity * t);
    }

    private void alightToPosition()
    {
        float maxTurnRate = (agility * Physics.gravity.magnitude)/missile_rb.velocity.magnitude;
        var dir = Vector3.RotateTowards(missile_rb.rotation * Vector3.forward, interceptionPosition - missile_rb.position, maxTurnRate * Time.deltaTime, 0);
        missile_rb.rotation = Quaternion.LookRotation(dir);
    }

    private void thrust()
    {
        missile_rb.velocity = missile_rb.rotation * new Vector3(0, 0, thrustPower);
    }

    private void explode()
    {
        var explosionParticle = Instantiate(explosion, null);
        explosionParticle.transform.position = missile_rb.position;
        target.gameObject.SetActive(false);
    }

    private void Update()
    {
        

        //within missile view check
        Vector3 directionToTarget = interceptionPosition - missile_rb.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        
        if(angle < lockAngle)
        {
            alightToPosition();
        }
        if(hit == false && Vector3.Distance(target.position, transform.position) <= detonationRange)
        {
            print("target hit");
            explode();
            Destroy(gameObject);
        }
        
    }
    private void FixedUpdate()
    {
        calculateTargetPosition();

        if(hit == false)
        {
            thrust();
        }
        
    }
}
